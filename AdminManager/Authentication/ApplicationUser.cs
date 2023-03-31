using AdminManager.Models;
using Microsoft.AspNetCore.Identity;

namespace AdminManager.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public int? PinCode { get; set; }
        public string? IsActive { get; set; }
        public SType? StatusType { get; set; }
        public string Reason { get; set; }

    }
}
