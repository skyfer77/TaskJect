using TaskJect.Web.Common;
using TaskJect.Web.Services;
using Data;
using Domain.Database;
using Domain.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
    [ApiController]
    public class GumroadController : Controller
    {
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly IOrganizationLimitationsEnforcer _organizationLimitationsEnforcer;
        private readonly IGumroadWebhookLogRepository _gumroadWebhookLogRepository;
        private readonly AesEncryptionHelper _encryptor;
        private readonly ILogger<GumroadController> _logger;
        public GumroadController(ITariffPlanHistoryRepository tariffPlanHistoryRepository, AesEncryptionHelper aesEncryption,
            IGumroadWebhookLogRepository gumroadWebhookLogRepository, IOrganizationLimitationsEnforcer organizationLimitationsEnforcer,
            ILogger<GumroadController> logger)
        {
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _organizationLimitationsEnforcer = organizationLimitationsEnforcer;
            _encryptor = aesEncryption;
            _logger = logger;
        }

        [HttpPost]
        [Route("api/gumroad/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromForm] IFormCollection payload, CancellationToken ct)
        {
            var organizationCode = getOrganizationCode(payload);
            if(organizationCode == Guid.Empty)
            {
                _logger.LogError("Cant find organization code for gumroad action");
                return Ok();
            }

            var (eventId, type, saleId, subscriptionId) = GumroadWebhookHelper.BuildEventIdAndType(payload);

            // перевіримо, чи вже обробляли цю подію
            if (await _gumroadWebhookLogRepository.ExistsAsync(eventId, ct))
            {
                return Ok();
            }

            // зафіксуємо обробку події — першим кроком
            await _gumroadWebhookLogRepository.AddAsync(new GumroadWebhookLog
            {
                EventId = eventId,
                EventType = type,
                SaleId = saleId,
                SubscriptionId = subscriptionId,
                OrganizationCode = organizationCode == Guid.Empty ? (Guid?)null : organizationCode,
                ProcessedAtUtc = DateTime.UtcNow
            }, ct);


            var subscriptionCode = payload["subscription_id"].ToString();
            var periodName = payload["recurrence"].ToString();
            var utcToday = DateTime.UtcNow.Date;

            var isRefunded = payload["refunded"].ToString().ToLower() == "true"; 
            bool isCancelled = payload["cancelled"].ToString().ToLower() == "true"
                            || payload["subscription_cancelled"].ToString().ToLower() == "true";

            if(isCancelled)
            {
                var endDateStr = payload["subscription_ended_at"].ToString();
                DateTime endDate;

                if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out var parsed))
                {
                    endDate = parsed.ToUniversalTime().Date;
                }
                else
                {
                    var currentPlan = await _tariffPlanHistoryRepository.Retrieve(subscriptionCode);
                    endDate = currentPlan?.DateTo ?? utcToday;
                }

                var defaultPlan = new TariffPlanHistoryDto
                {
                    OrganizationCode = organizationCode,
                    TariffPlanCode = SD.BasicPlanCode,
                    DateFrom = endDate,
                    DateTo = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    SubscriptionCode = subscriptionCode
                };

                await _organizationLimitationsEnforcer.ApplyTariffPlan(defaultPlan, false);
                return Ok();
            }
            if (isRefunded)
            {
                var newPlan = new TariffPlanHistoryDto(); 
                newPlan.TariffPlanCode = SD.BasicPlanCode;
                newPlan.DateTo = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);  
                var subsTatiffPlan = await _tariffPlanHistoryRepository.Retrieve(subscriptionCode);
                if(subsTatiffPlan != null)
                {
                    newPlan.DateFrom = utcToday;
                    newPlan.OrganizationCode = subsTatiffPlan.OrganizationCode;
                    await _organizationLimitationsEnforcer.ApplyTariffPlan(newPlan, true);
                }
                else
                {
                    _logger.LogError("Subscription not found for Gumroad");
                    return Ok ();
                }
            }
            else //це підписка, яка активується або продовжується
            {
                var newPlan = new TariffPlanHistoryDto();
                var shortProductId = payload["short_product_id"].ToString();

                newPlan.OrganizationCode = organizationCode;
                newPlan.DateFrom = utcToday;
                newPlan.DateTo = periodName == "yearly" ? newPlan.DateFrom.AddYears(1) : newPlan.DateFrom.AddMonths(1);

                var tierNameRaw = payload.TryGetValue("variants", out var v1) ? v1.ToString()
                   : payload.TryGetValue("variant", out var v2) ? v2.ToString()
                   : string.Empty;
                var tierName = (tierNameRaw ?? string.Empty).Trim();
                var tierToPlan = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Starter"] = SD.StarterPlanCode,
                    ["Pro"] = SD.ProPlanCode,
                    ["Business"] = SD.BusinessPlanCode,
                    ["Enterprise"] = SD.EnterprisePlanCode
                };

                newPlan.TariffPlanCode = tierToPlan.TryGetValue(tierName, out var code)
                            ? code
                            : SD.BasicPlanCode;
                newPlan.SubscriptionCode = subscriptionCode;
                await _organizationLimitationsEnforcer.ApplyTariffPlan(newPlan, false);
            }
            return Ok();
        }

        private Guid getOrganizationCode(IFormCollection payload)
        {
            if(payload.ContainsKey("url_params[custom]"))
            {
                var customValue = payload["url_params[custom]"].ToString();
                return _encryptor.Decrypt(customValue);
            }
            return Guid.Empty;
        }
    }

}
