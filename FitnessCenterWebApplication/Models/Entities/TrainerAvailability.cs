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
        public DayOfWeek DayOfWeek { get; set; } 

        [Required]
        public TimeSpan StartTime { get; set; } 

        [Required]
        public TimeSpan EndTime { get; set; } 

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey(nameof(TrainerId))]
        public Trainer Trainer { get; set; } = null!;
    }
}