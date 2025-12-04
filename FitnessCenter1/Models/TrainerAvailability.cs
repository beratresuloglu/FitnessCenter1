using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class TrainerAvailability
    {
        [Key]
        public int AvailabilityID { get; set; }
        public int TrainerID { get; set; }
        public string Day { get; set; }
        public TimeSpan StartingTime { get; set; } 
        public TimeSpan FinishingTime { get; set; } 
    }
}
