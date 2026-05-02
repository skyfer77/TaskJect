using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class CreateUserModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "FirstName", ResourceType = typeof(SharedResources))]
        public string FirstName { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Surname", ResourceType = typeof(SharedResources))]
        public string Surname { get; set; }
    }
}
