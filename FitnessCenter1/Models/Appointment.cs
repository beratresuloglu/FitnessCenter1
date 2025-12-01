namespace FitnessCenter1.Models
{
    public class Appointment //Randevu
    {
        public int AppointmentID { get; set; }
        public int FitnessCenterID { get; set; }
        public int MemberID { get; set; }
        public int TrainerID { get; set; }
        public int ServiceID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartingTime { get; set; }
        public TimeSpan FinishingTime { get; set; }
        public Enum Case { get; set; }
        public int Price { get; set; }
    }
}
