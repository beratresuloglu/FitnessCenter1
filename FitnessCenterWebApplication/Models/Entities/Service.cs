using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre zorunludur")]
        [Range(15, 240, ErrorMessage = "Süre 15-240 dakika arasında olmalıdır")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 arasında olmalıdır")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Lütfen bir spor salonu seçiniz")]
        public int? GymCenterId { get; set; }


        [ForeignKey(nameof(GymCenterId))]
        public GymCenter GymCenter { get; set; } = null!;

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}