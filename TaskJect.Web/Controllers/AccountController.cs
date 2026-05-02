using Domain.Database;
using TaskJect.Web.Models;
using TaskJect.Web.Models.Account;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace TaskJect.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IRegistarionOrganization _registarionOrganization;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
			IEmailService emailService, IStringLocalizer<ErrorResources> localizer,IRegistarionOrganization registarionOrganization, 
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _localizer = localizer;
            _registarionOrganization = registarionOrganization;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
			if (!ModelState.IsValid)
            {
                return View(model);
            }

			var result = await _registarionOrganization.RegistarionNewOrganization(model);
			if (!result.IsSuccess)
			{
				foreach (var error in result.Errors)
                {
					ModelState.AddModelError(string.Empty, error);
				}
					
				return View(model);
			}

            var user = await _userManager.FindByEmailAsync(model.Email);

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

			var emailParams = new AccountEmailParams
			{
				Type = AccountEmailType.ConfirmEmail,
				Email = model.Email,
				CallbackUrl = callbackUrl,
			};

			await _emailService.SendEmailAsync(emailParams);

			return RedirectToAction("RegistrationConfirm", "Account");
		}

        [HttpGet]
        public IActionResult RegistrationConfirm()
        {
            return View();
        }

		[HttpGet]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
            var response = new ServerResponse(false);

			if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
			{
                response.IsSuccess = false;
				response.Message = _localizer["EmailConfirmationFailed"];

				return View(response);
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				response.IsSuccess = false;
				response.Message = _localizer["EmailConfirmationFailed"];

				return View(response);
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				response.IsSuccess = true;
				response.Message = _localizer["EmailConfirmedSuccessfully"];

				return View(response);
			}

			response.IsSuccess = false;
			response.Message = _localizer["EmailConfirmationFailed"];

			return View(response);
		}

		[HttpGet]
        public IActionResult GetCookies()
        {
            var cookies = Request.Cookies;

            var cookieList = cookies.Select(c => new {
                Name = c.Key
            }).ToList();

            return Ok(cookieList);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
		//[ValidateAntiForgeryToken] - після скиданню паролю не буде пускати поки не оновиш сторінку
		//на логін View не використовується @Html.AntiForgeryToken()
		public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
						ModelState.AddModelError(string.Empty, _localizer["EmailNotConfirmed"]);
						return View(model);
                    }

					var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
					if (result.Succeeded)
					{
						if (user.IsNewUser)
						{
							var code = await _userManager.GeneratePasswordResetTokenAsync(user);
							return RedirectToAction("ResetPassword", "Account", new { userId = user.Id, code = code });
						}
						else
						{
							return RedirectToAction("Index", "Profile");
						}
					}
				}
                ModelState.AddModelError(string.Empty, _localizer["InvalidLoginAttempt"]);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return View("Logout");
            //return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                var emailParams = new AccountEmailParams
                {
					Type = AccountEmailType.ResetPassword,
					Email = model.Email,
                    CallbackUrl = callbackUrl,
                };

                await _emailService.SendEmailAsync(emailParams);

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string userId, string code = null)
        {
            if (userId == null || code == null)
            {
                return BadRequest(_localizer["EmailCodeProvided"]);
            }
            var user = await _userManager.FindByIdAsync(userId);
            var model = new ResetPasswordViewModel { Email = user.Email, Code = code };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, _localizer["UserWithEmailNotExist"]);
                return View(model);
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                user.IsNewUser = false;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("ResetPasswordConfirmation");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel changePassword)
        {
            if (!ModelState.IsValid)
            {
	            return Json(new ServerResponse(false) { Message = _localizer["DefaultError"] });
            }

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["UserWithEmailNotExist"] });
            }

            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, changePassword.CurrentPassword);
            if (!isCorrectPassword)
            {
	            return Json(new ServerResponse(false) { Message = _localizer["PasswordMismatch"] });
            }

            var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                return Json(new ServerResponse(true) { Message = _localizer["PasswordChanged"] });
            }

            return Json(new ServerResponse(false) { Message = _localizer["DefaultError"] });
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
