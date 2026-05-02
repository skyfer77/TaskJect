using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models
{
    public class SignInViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}
