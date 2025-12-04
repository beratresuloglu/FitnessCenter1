using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class Service
    {
        [Key]
        public int ServiceID { get; set; }
        public int FitnessCenterID {  get; set; }
        public String ServiceName { get; set; }
        public int ServiceTime { get; set; } // hizmetin kaç dakika süreceği
        public int ServicePrice{ get; set; } // hizmetin kaç dakika süreceği

    }
}
