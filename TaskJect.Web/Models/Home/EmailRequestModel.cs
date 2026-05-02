using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class EmailRequestModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "FullName", ResourceType = typeof(SharedResources))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Phone", ResourceType = typeof(SharedResources))]
        public string Phone { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Email", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Message", ResourceType = typeof(SharedResources))]
        public string Message { get; set; }
    }
}
