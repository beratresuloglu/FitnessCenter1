using Microsoft.AspNetCore.Identity;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Member? Member { get; set; }
        public Trainer? Trainer { get; set; }
    }
}