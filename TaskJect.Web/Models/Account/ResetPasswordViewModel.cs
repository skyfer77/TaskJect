using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SharedResources))]
        [Compare("Password", ErrorMessageResourceName = "ComparePasswordError",
                ErrorMessageResourceType = typeof(ErrorResources))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
