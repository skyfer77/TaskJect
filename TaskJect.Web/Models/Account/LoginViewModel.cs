// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        //[EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid",
        //            ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(SharedResources))]
        public bool RememberMe { get; set; }

    }
}