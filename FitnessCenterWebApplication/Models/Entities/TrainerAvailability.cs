using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class TrainerAvailability
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; } // 0: Pazar, 1: Pazartesi...

        [Required]
        public TimeSpan StartTime { get; set; } // Örn: 09:00

        [Required]
        public TimeSpan EndTime { get; set; }   // Örn: 17:00

        // Controller'daki kodlarla uyumlu olması için IsActive ismini kullanıyoruz.
        // Bu kayıt aktif bir vardiya tanımı mı? (Silindiğinde false olur)
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(TrainerId))]
        public Trainer Trainer { get; set; } = null!;
    }
}