using Domain.Database;
using Microsoft.AspNetCore.Identity;

namespace TaskJect.Web.Services
{
    public class UserCreator : IUserCreator
    {
        private readonly IApplicationUserRepository _userRepository;
        private readonly IEmailService _emailService;

		public UserCreator(
        IApplicationUserRepository userRepository,
		IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
		}

        public async Task<bool> CreateUser(CreateUserByEmailModel model)
        {
            var tempPassword = generateSecureTemporaryPassword();

            var result = await _userRepository.CreateUser(model, tempPassword);

            if (result)
            {
				var emailParams = new AccountEmailParams
				{
					Type = AccountEmailType.AccountCreated,
					Email = model.Email,
                    TempPassword = tempPassword,
				};

				await _emailService.SendEmailAsync(emailParams);
			}

            return result;
        }

        private string generateSecureTemporaryPassword()
        {
            var options = new PasswordOptions
            {
                RequiredLength = 12,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true
            };

            string[] randomChars = new[]
            {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",
            "abcdefghijkmnopqrstuvwxyz",
            "0123456789",
            "!@$?_-"
        };

            var rand = new Random();
            var chars = new List<char>
        {
            randomChars[0][rand.Next(randomChars[0].Length)],
            randomChars[1][rand.Next(randomChars[1].Length)],
            randomChars[2][rand.Next(randomChars[2].Length)],
            randomChars[3][rand.Next(randomChars[3].Length)]
        };

            for (int i = chars.Count; i < options.RequiredLength; i++)
            {
                string rcs = randomChars[rand.Next(randomChars.Length)];
                chars.Add(rcs[rand.Next(rcs.Length)]);
            }

            return new string(chars.OrderBy(x => rand.Next()).ToArray());
        }
    }
}
