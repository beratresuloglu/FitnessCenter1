namespace FitnessCenter1.Models
{
    public class TrainerAvailability
    {
        public int AvailabilityID { get; set; }
        public int TrainerID { get; set; }
        public string Day { get; set; }
        public TimeSpan StartingTime { get; set; } 
        public TimeSpan FinishingTime { get; set; } 
    }
}
