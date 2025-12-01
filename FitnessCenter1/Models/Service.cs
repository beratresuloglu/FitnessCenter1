namespace FitnessCenter1.Models
{
    public class Service
    {
        public int ServiceID { get; set; }
        public int FitnessCenterID {  get; set; }
        public String ServiceName { get; set; }
        public int ServiceTime { get; set; } // hizmetin kaç dakika süreceği
        public int ServicePrice{ get; set; } // hizmetin kaç dakika süreceği

    }
}
