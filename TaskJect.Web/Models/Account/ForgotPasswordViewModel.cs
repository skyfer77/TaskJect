using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }
    }
}
