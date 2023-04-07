using System.ComponentModel.DataAnnotations;

namespace AdminManager.Models
{
    public  class Register
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int PinCode { get; set; }  
        public string? PhoneNumber { get; set; }
    }
}
