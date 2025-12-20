using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class GymCenter
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public TimeSpan OpenTime { get; set; }

        [Required]
        public TimeSpan CloseTime { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}