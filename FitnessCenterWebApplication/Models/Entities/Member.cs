using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class Member 
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        public string? Address { get; set; }

        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        // Fitness bilgileri
        public decimal? Height { get; set; } // cm
        public decimal? Weight { get; set; } // kg

        [NotMapped]
        public decimal? BMI => Height.HasValue && Weight.HasValue && Height > 0
    ? Math.Round(Weight.Value / ((Height.Value / 100) * (Height.Value / 100)), 2)
    : null;

        public string? FitnessGoal { get; set; }

        public string? MedicalConditions { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime JoinDate { get; set; } = DateTime.Now;

        public DateTime? MembershipExpiry { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    }
}