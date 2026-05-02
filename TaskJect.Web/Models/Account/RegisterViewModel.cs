using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;
using Data;
namespace TaskJect.Web.Models
{
    public class RegisterViewModel
    {
		//[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
		//    ErrorMessageResourceType = typeof(ErrorResources))]
		//[Display(Name = "UserName", ResourceType = typeof(SharedResources))]
		//public string Username { get; set; }
		[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
			ErrorMessageResourceType = typeof(ErrorResources))]
		[Display(Name = "OrganizationName", ResourceType = typeof(SharedResources))]
		public string OrganizationName { get; set; }

		[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string Password { get; set; }
		[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
			ErrorMessageResourceType = typeof(ErrorResources))]
		[DataType(DataType.Password)]
		[Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
		[Compare("Password", ErrorMessageResourceName = "ComparePasswordError",
				ErrorMessageResourceType = typeof(ErrorResources))]
		public string ConfirmPassword { get; set; }

		//public string ReturnUrl { get; set; }
        public string RoleName { get; set; } = SD.User;

        public bool AllowRememberLogin { get; set; } = true;
        public bool EnableLocalLogin { get; set; } = true;

        public bool IsExternalLoginOnly => EnableLocalLogin == false;
        public string ExternalLoginScheme { get; set; } = "Cookies";
       /* public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;*/


    }
}
