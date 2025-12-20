using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class Trainer
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

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; }

        public int ExperienceYears { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime HireDate { get; set; } = DateTime.Now;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int GymCenterId { get; set; }

        public string? UserId { get; set; }

        [ForeignKey(nameof(GymCenterId))]
        public GymCenter GymCenter { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}