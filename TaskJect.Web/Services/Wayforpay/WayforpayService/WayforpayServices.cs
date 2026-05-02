using AutoMapper;
using TaskJect.Web.Enums;
using TaskJect.Web.Models;
using Data;
using Domain.Database;
using Domain.Enums;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TaskJect.Web.Services
{
	public class RegularPaymentStatus
	{
		public string Status { get; set; } = null!;
		public DateTime DateNext { get; set; }
	}

	public class MerchantData
	{
		public string OrganizationCode { get; set; }
		public string UserId { get; set; }
		public string PlanCode { get; set; }
	}

	public class WayforpayServices : IWayforpayServices
	{
		private readonly IPaymentWayForPayRepository _wayForPayRepository;
		private readonly ITariffPlanRepository _tariffPlanRepository;
		private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
		private readonly ILogger<WayforpayServices> _logger;
		private readonly IMapper _mapper;

		private readonly string _domain;
		private readonly string _merchantAccount;
		private readonly string _merchantPassword;
		private readonly string _merchantSecret;
		private readonly string _merchantDomainName;
		private readonly string _currency;
		private readonly string _currencyRatesPath;
		private readonly string _urlPay;
		private readonly string _urlRegularApi;
		private readonly string _urlApi;
		private readonly string _serviceUrl;
		private readonly string _returnUrl;

		public WayforpayServices(IPaymentWayForPayRepository wayForPayRepository, ITariffPlanRepository tariffPlanRepository,
			ITariffPlanHistoryRepository tariffPlanHistoryRepository, IConfiguration config, ILogger<WayforpayServices> logger, 
			IMapper mapper)
		{
			_wayForPayRepository = wayForPayRepository;
			_tariffPlanRepository = tariffPlanRepository;
			_tariffPlanHistoryRepository = tariffPlanHistoryRepository;
			_logger = logger;
			_mapper = mapper;

			_domain = config["Domain"];
			_merchantAccount = config["WayForPay:MerchantAccount"];
			_merchantPassword = config["WayForPay:MerchantPassword"];
			_merchantSecret = config["WayForPay:MmerchantSecret"];
			_merchantDomainName = config["WayForPay:MerchantDomainName"];
			_currency = config["WayForPay:Currency"];
			_currencyRatesPath = config["WayForPay:CurrencyRatesPath"];
			_urlPay = config["WayForPay:Url:Pay"];
			_urlRegularApi = config["WayForPay:Url:RegularApi"];
			_urlApi = config["WayForPay:Url:Api"];
			_serviceUrl = config["WayForPay:ServiceUrl"];
			_returnUrl = config["WayForPay:ReturnUrl"];
		}

		public async Task<string?> CreateRegularPaymentAsync(WayforpaySubscriptionView subscription)
		{
			var payload = await createPayloadAsync(subscription);
			var htmlForm = generateWayforpayFormHtml(payload);

			var result = await savePayment(payload, subscription.PeriodType);

			return result ? htmlForm : null;
		}

		private async Task<Dictionary<string, string>> createPayloadAsync(WayforpaySubscriptionView subscription)
		{
			var orderDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			var orderReference = createSignatureHmac(_merchantSecret, orderDate, subscription.OrganizationCode, subscription.PlanCode);

			var regularMode = subscription.PeriodType == SubscriptionPeriodTypeView.Year ? "yearly" : "monthly";

			var amountUSD = await getAmountTariff(subscription.PlanCode, regularMode);

			var usd = await GetCurrencyRatesAsync("USD");
			var amount = (amountUSD * usd).ToString("F2", CultureInfo.InvariantCulture);

			var dateNext = subscription.PeriodType == SubscriptionPeriodTypeView.Year
						? DateTime.UtcNow.AddYears(1).ToString("dd.MM.yyyy")
						: DateTime.UtcNow.AddMonths(1).ToString("dd.MM.yyyy");

			string dateEnd = subscription.PeriodType == SubscriptionPeriodTypeView.Year
						? DateTime.UtcNow.AddYears(3).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
						: DateTime.UtcNow.AddYears(1).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

			string signature = createSignatureHmac(_merchantSecret, _merchantAccount, _merchantDomainName, 
				orderReference, orderDate, amount, _currency, subscription.PlanName, 1, amount);

			var merchantData = new MerchantData
			{
				UserId = subscription.UserId,
				OrganizationCode = subscription.OrganizationCode,
				PlanCode = subscription.PlanCode,
			};

			var payload = new Dictionary<string, string>
			{
				{ "merchantAccount", _merchantAccount },
				{ "merchantAuthType", "SimpleSignature" },
				{ "merchantDomainName", _merchantDomainName },
				{ "merchantSignature", signature },
				{ "orderReference", orderReference }, //Унікальний номер замовлення в системі торговця
				{ "orderDate", orderDate.ToString() },
				{ "amount", amount },//Ціна на товар вказано обовязково
				{ "currency", _currency },
				{ "productName[]", subscription.PlanName },
				{ "productPrice[]", amount },
				{ "productCount[]", "1" },
				{ "regularBehavior", "preset" },//  - preset - щоб клієнт не зміг редагувати параметри регулярного платежу на платіжній сторінці
				{ "regularMode", regularMode }, //- monthly - щомісяця або - yearly - раз на рік
				{ "regularAmount", amount },
				{ "regularOn", "1" },//При значеннi = 1, чекбокс "зробити платiж регулярним" активований, regularAmount заблокованоо для редагування.
				{ "dateNext", dateNext },//Дата першого списання регулярного платежу в форматі ДД.ММ.РРРР. Дата повинна бути більше поточної дати
				{ "dateEnd", dateEnd },
				{ "merchantData", JsonSerializer.Serialize(merchantData) },
				{ "serviceUrl", _domain + _serviceUrl },
				{ "returnUrl", _domain + _returnUrl }
			};

			return payload;
		}

		private async Task<decimal> getAmountTariff(string planCode, string period)
		{
			var tariff = await _tariffPlanRepository.Retrieve(planCode);
			if (tariff == null)
			{
				throw new Exception($"The tariff plan with code '{planCode}' was not found or is inactive.");
			}

			if (period == "yearly")
			{
				// В базі зі скидкою на місяць ціна треба за рік
				return parseStringToDecimal(tariff.PriceYearlyDiscount) * 12;
			}
			else
			{
				if (tariff.Source == SD.TariffPlanSource)
				{
					return parseStringToDecimal(tariff.PriceMonthlyDiscount);
				}
				else
				{
					return parseStringToDecimal(tariff.PriceMonth);
				}
			}
		}

		private decimal parseStringToDecimal(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
			{
				return 0m;
			}

			str = str.Replace("$", "").Trim();

			return decimal.Parse(str, CultureInfo.InvariantCulture);
		}

		private string generateWayforpayFormHtml(Dictionary<string, string> payload)
		{
			var sb = new StringBuilder();

			sb.AppendLine("<html>");
			sb.AppendLine("<body onload='document.forms[0].submit()'>");//Авто сабміт 
			sb.AppendLine($"<form method='POST' action='{_urlPay}'>");

			foreach (var kvp in payload)
			{
				var name = System.Net.WebUtility.HtmlEncode(kvp.Key);
				var value = System.Net.WebUtility.HtmlEncode(kvp.Value);

				sb.AppendLine($"<input type='hidden' name='{name}' value='{value}' />");
			}

			sb.AppendLine("</form>");
			sb.AppendLine("</body>");
			sb.AppendLine("</html>");

			return sb.ToString();
		}

		private async Task<bool> savePayment(Dictionary<string, string> payload, SubscriptionPeriodTypeView periodType)
		{
			var payment = convertStringToPaymentWayForPayDto(payload, periodType);
			var result = await _wayForPayRepository.InsertAsync(payment);

			return result;
		}

		private PaymentWayForPayDto convertStringToPaymentWayForPayDto(Dictionary<string, string> payload, SubscriptionPeriodTypeView periodType)
		{
			string merchantDataJson = payload.ContainsKey("merchantData") ? payload["merchantData"] : "{}";
			var merchantData = JsonSerializer.Deserialize<Dictionary<string, string>>(merchantDataJson);

			var payment = new PaymentWayForPayDto
			{
				UserId = merchantData != null && merchantData.ContainsKey("UserId") ? merchantData["UserId"] : null,
				OrganizationCode = merchantData != null && merchantData.ContainsKey("OrganizationCode") ? merchantData["OrganizationCode"] : null,
				PlanCode = merchantData != null && merchantData.ContainsKey("PlanCode") ? merchantData["PlanCode"] : null,
				SubscriptionPeriod = _mapper.Map<SubscriptionPeriodType>(periodType),
				OrderReference = payload.ContainsKey("orderReference") ? payload["orderReference"] : null,
				Amount = payload.ContainsKey("amount") ? decimal.Parse(payload["amount"], CultureInfo.InvariantCulture) : 0m,
				Currency = payload.ContainsKey("currency") ? payload["currency"] : null,
				Status = "Created",
				DateNext = payload.ContainsKey("dateNext")
					? DateTime.ParseExact(payload["dateNext"], "dd.MM.yyyy", CultureInfo.InvariantCulture) : null,
				CreatedAt = DateTime.UtcNow,
				RecToken = null
			};

			return payment;
		}

		public async Task<bool> CancelSubscriptionAsync(string organizationCode)
		{
			try
			{
				var planActive = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));
				if (planActive?.SubscriptionCode == null)
				{
					return true;
				}

				var payload = new Dictionary<string, string>
				{
					{ "requestType", "REMOVE"},
					{ "merchantAccount", _merchantAccount },
					{ "merchantPassword", _merchantPassword },
					{ "orderReference", planActive.SubscriptionCode },
				};
			
				var json = await sendReqularApi(payload);
				if (!json.TryGetProperty("reasonCode", out var reasonProp))
				{
					_logger.LogWarning("CancelSubscriptionAsync: Answer without reasonCode. JSON: {Json}", json.ToString());
					return false;
				}

				var reasonCode = reasonProp.GetInt32();

				// 4100 - OK
				// 4102 - не знайдено регулярний платіж за вказаним номером замовлення
				// 4106 - Регулярний платіж завершено
				// 4107 - Регулярний платіж закрито
				if (reasonCode == 4100 || reasonCode == 4102 || reasonCode == 4106 || reasonCode == 4107)
				{
					return true;
				}

				_logger.LogWarning("CancelSubscriptionAsync: unknown reasonCode={ReasonCode} for org={OrgCode}", reasonCode, organizationCode);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error when canceling a recurring payment for WayForPay org={OrgCode}", organizationCode);
				throw new Exception($"❌ Error when canceling a recurring payment for WayForPay org={organizationCode}");
			}
		}

		public async Task<string?> ChangePlanAsync(WayforpaySubscriptionView newSubscription)
		{
			var orgId = Guid.Parse(newSubscription.OrganizationCode);
			var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(orgId);

			if (activePlan != null)
			{
				var removed = await CancelSubscriptionAsync(newSubscription.OrganizationCode);
				if (!removed)
				{
					return null;
				}
			}

			var html = await CreateRegularPaymentAsync(newSubscription);
			return html;
		}

		public async Task<RegularPaymentStatus?> GetRegularPaymentStatusAsync(string orderReference)
		{
			try
			{
				var payload = new Dictionary<string, string>
				{
					{ "requestType", "STATUS"},
					{ "merchantAccount", _merchantAccount },
					{ "merchantPassword", _merchantPassword },
					{ "orderReference", orderReference },
				};

				var json = await sendReqularApi(payload);

				if (!json.TryGetProperty("reasonCode", out var reasonProp))
				{
					_logger.LogWarning("GetRegularPaymentStatusAsync: Answer without reasonCode. JSON: {Json}", json.ToString());
					return null;
				}

				var reasonCode = reasonProp.GetInt32();
				if (reasonCode != 4100 && reasonCode != 1100)
				{
					_logger.LogWarning("GetRegularPaymentStatusAsync: reasonCode={ReasonCode}, order={OrderRef}", reasonCode, orderReference);
					return null;
				}

				var status = json.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;

				DateTime? nextDate = null;
				if (json.TryGetProperty("nextPaymentDate", out var nextProp))
				{
					try
					{
						nextDate = DateTimeOffset.FromUnixTimeSeconds(nextProp.GetInt64()).DateTime;
					}
					catch
					{
						_logger.LogWarning("GetRegularPaymentStatusAsync: incorrect date nextPaymentDate for order={OrderRef}", orderReference);
					}
				}

				return new RegularPaymentStatus
				{
					Status = status,
					DateNext = nextDate.Value
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error while getting WayForPay regular payment status for order={OrderRef}", orderReference);
				return null;
			}
		}

		private async Task<JsonElement> sendReqularApi(Dictionary<string, string> payload)
		{
			using var client = new HttpClient();
			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{_urlRegularApi}", content);

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"WayForPay Regular API error: {response.StatusCode}");
			}

			var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

			return json;
		}

		public bool VerifySignature(JsonElement json)
		{
			try
			{
				if (!json.TryGetProperty("merchantSignature", out var signatureProp))
				{
					return false;
				}

				string receivedSignature = signatureProp.GetString() ?? string.Empty;
				string merchantAccount = json.TryGetProperty("merchantAccount", out var ma) ? ma.GetString() ?? "" : "";
				string orderReference = json.TryGetProperty("orderReference", out var orf) ? orf.GetString() ?? "" : "";
				decimal amount = json.TryGetProperty("amount", out var amt) && amt.TryGetDecimal(out var dec) ? dec : 0;
				string currency = json.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "" : "";
				string authCode = json.TryGetProperty("authCode", out var ac) ? ac.GetString() ?? "" : "";
				string cardPan = json.TryGetProperty("cardPan", out var cp) ? cp.GetString() ?? "" : "";
				string transactionStatus = json.TryGetProperty("transactionStatus", out var ts) ? ts.GetString() ?? "" : "";
				int reasonCode = json.TryGetProperty("reasonCode", out var rc) && rc.TryGetInt32(out var rcInt) ? rcInt : 0;

				string localSignature = createSignatureHmac(
					_merchantSecret,
					merchantAccount,
					orderReference,
					amount.ToString("0.##", CultureInfo.InvariantCulture),
					currency,
					authCode,
					cardPan,
					transactionStatus,
					reasonCode.ToString()
				);

				return string.Equals(localSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ VerifySignature: error while verifying the WayForPay signature. Data: {Json}", json.ToString());
				return false;
			}
		}

		public async Task<decimal> GetCurrencyRatesAsync(string currencyCode)
		{
			var currencyRate = await tryGetRateFromFileAsync(currencyCode);

			if (currencyRate != null)
			{
				return currencyRate.Value;
			}

			var usdRate = await fetchRateFromWayForPayAsync();

			var fetchedRates = await fetchRateFromWayForPayAsync();
			if (fetchedRates == null || fetchedRates.Count == 0)
			{
				var lastUsdRate = await tryGetRateFromFileAsync(currencyCode, false);
				if (lastUsdRate != null)
				{
					return lastUsdRate.Value;
				}

				throw new Exception($"Currency rates was not found or is inactive.");
			}

			await saveRatesToFileAsync(fetchedRates);

			var updatedRate = await tryGetRateFromFileAsync(currencyCode);
			return updatedRate.Value;
		}

		#region Currency Rates
		private async Task<decimal?> tryGetRateFromFileAsync(string currencyCode, bool checkDate = true)
		{
			if (!File.Exists(_currencyRatesPath))
			{
				return null;
			}
				
			try
			{
				var json = await File.ReadAllTextAsync(_currencyRatesPath);
				var rates = JsonSerializer.Deserialize<CurrencyRate>(json);

				if (rates == null || rates.Rates == null)
				{
					return null;
				}

				if (checkDate && rates.Date.Date != DateTime.UtcNow.Date)
				{
					return null;
				}

				if (rates.Rates.TryGetValue(currencyCode, out var rate))
				{
					return rate;
				}
					
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Unexpected error while Get Currency Rates: {Message}", ex.Message);
			}

			return null;
		}

		private async System.Threading.Tasks.Task saveRatesToFileAsync(Dictionary<string, decimal> rates)
		{
			var currencyRate = new CurrencyRate
			{
				Date = DateTime.UtcNow,
				Rates = rates
			};

			var options = new JsonSerializerOptions { WriteIndented = true };
			await File.WriteAllTextAsync(_currencyRatesPath, JsonSerializer.Serialize(currencyRate, options));
		}

		private async Task<Dictionary<string, decimal>?> fetchRateFromWayForPayAsync()
		{
			var orderDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var signature = createSignatureHmac(_merchantSecret, _merchantAccount, orderDate);

			var payload = new
			{
				apiVersion = 1,
				transactionType = "CURRENCY_RATES",
				merchantAccount = _merchantAccount,
				merchantSignature = signature,
				orderDate = orderDate
			};

			using var client = new HttpClient();
			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			var response = await client.PostAsync($"{_urlApi}", content);
			var resultJson = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			return parseRates(resultJson);
		}

		private Dictionary<string, decimal>? parseRates(string resultJson)
		{
			try
			{
				var json = JsonSerializer.Deserialize<JsonElement>(resultJson);
				if (json.TryGetProperty("rates", out var ratesJson))
				{
					var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
					foreach (var property in ratesJson.EnumerateObject())
					{
						if (property.Value.ValueKind == JsonValueKind.Number &&
							property.Value.TryGetDecimal(out var value))
						{
							rates[property.Name.ToUpperInvariant()] = value;
						}
					}
					return rates;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"[WayForPay] Parse error: {ex.Message}");
			}

			return null;
		}

		private class CurrencyRate
		{
			public DateTime Date { get; set; }
			public Dictionary<string, decimal> Rates { get; set; } = new();
		}

		#endregion

		private string createSignatureHmac(string key, params object[] data)
		{
			var raw = string.Join(";", data);
			using var hmac = new HMACMD5(Encoding.UTF8.GetBytes(key));
			var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}
	}
}
