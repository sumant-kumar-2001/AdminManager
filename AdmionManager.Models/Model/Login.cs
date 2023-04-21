using System.ComponentModel.DataAnnotations;

namespace AdminManager.Models
{
    public class Login
    {
        [Required(ErrorMessage = "User Name is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
