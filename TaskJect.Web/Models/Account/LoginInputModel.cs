// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class LoginInputModel
    {
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "UserName", ResourceType = typeof(SharedResources))]
        public string Login { get; set; }
        [Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
                    ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(Name = "Password", ResourceType = typeof(SharedResources))]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}