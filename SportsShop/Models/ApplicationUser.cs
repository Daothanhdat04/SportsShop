using Microsoft.AspNetCore.Identity;

namespace SportsShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}