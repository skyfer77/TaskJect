using TaskJect.Web.Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Data;

namespace TaskJect.Web.Services
{
    public class GmailEmailSender : IEmailSender
    {
        private GmailService _gmailService;
        private GmailOptions _gmailOptions;
        private UserCredential _credential;
        private readonly ILogger<GmailEmailSender> _logger;

        public GmailEmailSender(IOptions<GmailOptions> options, ILogger<GmailEmailSender> logger)
        {
            _logger = logger;
            _gmailOptions = options.Value;
            initializeGmailService();
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (isTokenExpired())
            {
                await refreshAccessTokenAsync();
            }
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(SD.SenderEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("html") { Text = message };

            using var memoryStream = new MemoryStream();
            mimeMessage.WriteTo(memoryStream);
            var rawMessage = Convert.ToBase64String(memoryStream.ToArray())
                .Replace("+", "-").Replace("/", "_").Replace("=", "");


            var gmailMessage = new Message { Raw = rawMessage };
            try
            {
                var response = await _gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
            }
            catch (Google.GoogleApiException gEx)
            {
                _logger.LogError(gEx, "❌ Gmail API error: {Message}", gEx.Message);
                throw; // можна пробросити далі або погасити
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error while sending Gmail message: {Message}", ex.Message);
                throw;
            }
        }

        private void initializeGmailService(UserCredential userCredential = null)
        {
            if(userCredential == null)
            {
                userCredential = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _gmailOptions.ClientId,
                        ClientSecret = _gmailOptions.ClientSecret
                    }
                }), "user", new TokenResponse { AccessToken = _gmailOptions.AccessToken, RefreshToken = _gmailOptions.RefreshToken });
            }
            
            _credential = userCredential;

            _gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "Taskject server"
            });

            
        }
        private async Task refreshAccessTokenAsync()
        {
            var credential = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _gmailOptions.ClientId,
                    ClientSecret = _gmailOptions.ClientSecret
                }
            }), "user", new TokenResponse { RefreshToken = _gmailOptions.RefreshToken });
            var isSuccessRefreshing = await credential.RefreshTokenAsync(CancellationToken.None);
            if (isSuccessRefreshing)
            {
                _gmailOptions.AccessToken = credential.Token.AccessToken;

                if (!string.IsNullOrEmpty(credential.Token.RefreshToken))
                {
                    _gmailOptions.RefreshToken = credential.Token.RefreshToken;
                }
                _credential = credential;
                initializeGmailService(credential);
            }
        }

        private bool isTokenExpired()
        {
            if (_gmailOptions.AccessToken != null && _credential.Token.ExpiresInSeconds.HasValue)
            {
                var expirationTime = _credential.Token.IssuedUtc + TimeSpan.FromSeconds(_credential.Token.ExpiresInSeconds.Value);
                return expirationTime <= DateTime.UtcNow;
            }

            return false;
        }
    }
}
