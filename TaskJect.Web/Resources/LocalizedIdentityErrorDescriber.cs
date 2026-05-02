using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Resources
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<ErrorResources> _localizer;

        public LocalizedIdentityErrorDescriber(IStringLocalizer<ErrorResources> localizer)
        {
            _localizer = localizer;
        }

        public override IdentityError DefaultError()
            => new IdentityError { Code = nameof(DefaultError), Description = _localizer["DefaultError"] };

        public override IdentityError ConcurrencyFailure()
            => new IdentityError { Code = nameof(ConcurrencyFailure), Description = _localizer["ConcurrencyFailure"] };

        public override IdentityError PasswordMismatch()
            => new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer["PasswordMismatch"] };

        public override IdentityError InvalidToken()
            => new IdentityError { Code = nameof(InvalidToken), Description = _localizer["InvalidToken"] };

        public override IdentityError LoginAlreadyAssociated()
            => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer["LoginAlreadyAssociated"] };

        public override IdentityError InvalidUserName(string userName)
            => new IdentityError { Code = nameof(InvalidUserName), Description = _localizer["InvalidUserName", userName] };

        public override IdentityError InvalidEmail(string email)
            => new IdentityError { Code = nameof(InvalidEmail), Description = _localizer["InvalidEmail", email] };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = _localizer["DuplicateUserName", userName] };

        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = _localizer["DuplicateEmail", email] };

        public override IdentityError InvalidRoleName(string role)
            => new IdentityError { Code = nameof(InvalidRoleName), Description = _localizer["InvalidRoleName", role] };

        public override IdentityError DuplicateRoleName(string role)
            => new IdentityError { Code = nameof(DuplicateRoleName), Description = _localizer["DuplicateRoleName", role] };

        public override IdentityError UserAlreadyHasPassword()
            => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer["UserAlreadyHasPassword"] };

        public override IdentityError UserLockoutNotEnabled()
            => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer["UserLockoutNotEnabled"] };

        public override IdentityError UserAlreadyInRole(string role)
            => new IdentityError { Code = nameof(UserAlreadyInRole), Description = _localizer["UserAlreadyInRole", role] };

        public override IdentityError UserNotInRole(string role)
            => new IdentityError { Code = nameof(UserNotInRole), Description = _localizer["UserNotInRole", role] };

        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = _localizer["PasswordTooShort", length] };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["PasswordRequiresNonAlphanumeric"] };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer["PasswordRequiresDigit"] };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer["PasswordRequiresLower"] };

        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer["PasswordRequiresUpper"] };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = _localizer["PasswordRequiresUniqueChars", uniqueChars] };
   
    }
}
