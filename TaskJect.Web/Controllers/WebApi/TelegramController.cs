using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace TaskJect.Web.Controllers
{
    [ApiController]
    [Route("telegram")]
    public class TelegramController : Controller
    {
        private readonly ITelegramService _telegramService;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IStringLocalizer<ErrorResources> _errorLocalizer;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        public TelegramController(ITelegramService telegramService, ITariffPlanHistoryRepository tariffPlanHistoryRepository,
            IApplicationUserRepository applicationUserRepository, IStringLocalizer<SharedResources> localizer, 
            IStringLocalizer<ErrorResources> errorLocalizer, ITariffPlanRepository tariffPlanRepository)
        {
            _telegramService = telegramService;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _applicationUserRepository = applicationUserRepository;
            _localizer = localizer;
            _errorLocalizer = errorLocalizer;
            _tariffPlanRepository = tariffPlanRepository;
        }

        [HttpPost("update")]
        public async Task<IActionResult> ReceiveUpdate([FromBody] TelegramUpdate update)
        {
            if (update.Message == null)
            {
                return Ok();
            }

            var chatId = update.Message.Chat.Id.ToString();
            if (string.IsNullOrEmpty(chatId))
            {
                return Ok();
            }

            if (!long.TryParse(chatId, out var chatIdLong))
            {
                return Ok();
            }

            var messageText = update.Message.Text ?? string.Empty;

            var user = await _applicationUserRepository.GetUserByTelegramChatId(chatId);

            var culture = user?.Culture ?? "en";
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            if (user != null)
            {
                if (Guid.TryParse(user.OrganizationCode, out var guidOrganizationCode))
                {
                    var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(guidOrganizationCode);

                    if (activePlan == null)
                    {
                        await _telegramService.SendMessageAsync(chatId, _localizer["IntegrationViaTelegramAvailableOnlyUsersProPlan"]);
                        return Ok();
                    }
                    else
                    {
                        var tariffPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
                        if(tariffPlan == null || !tariffPlan.HasTelegramIntegration )
                        {
                            await _telegramService.SendMessageAsync(chatId, _localizer["IntegrationViaTelegramAvailableOnlyUsersProPlan"]);
                            return Ok();
                        }
                    }

                    processUserMessage(update.Message);

                    return Ok();
                }
                await _telegramService.SendMessageAsync(chatId, _localizer["WeCouldntFindOrganizationYourAccount"]);
                return Ok();

            }

            if (messageText.StartsWith("/start"))
            {
                var ticket = getTicketFromStartCommand(messageText);

                if (string.IsNullOrEmpty(ticket))
                {
                    await _telegramService.SendMessageAsync(chatId, _localizer["ToLinkYourAccountFollowPersonal"]);
                    return Ok();
                }

                var registrationResult = await _telegramService.RegisterUserByTicketAsync(chatId, ticket, update.Message.Chat.Username);

                if (registrationResult.Success)
                {
                    await _telegramService.SendMessageAsync(chatId, _localizer["CongratulationsYourAccountSuccessfullyLinkedTelegram"]);
                }
                else
                {
                    await _telegramService.SendMessageAsync(chatId,
                        registrationResult.ErrorMessage ?? _errorLocalizer["AnUnknownErrorOccurredWhileLinkingAccount"]);
                }
            }
            else
            {
                await _telegramService.SendMessageAsync(chatId, _localizer["PleaseUseYourPersonalizedLink"]);
            }

            return Ok();
        }

        private string? getTicketFromStartCommand(string startCommand)
        {
            var parts = startCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return parts[1];
            }
            return null;
        }

        private async void processUserMessage(TelegramMessage telegramMessage)
        {
            await _telegramService.SendMessageAsync(telegramMessage.Chat.Id.ToString(),
                            _localizer["ICanOnlyInformYouButNotEngageDialog"]);
        }
    }
}
