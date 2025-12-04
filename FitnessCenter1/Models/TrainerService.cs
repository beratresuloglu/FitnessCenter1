using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class TrainerService
    {
        [Key]
        public int TrainerServiceID { get; set; }
        public int TrainerID { get; set; } //Fk
        public int ServiceID { get; set; } //Fk
    }
}
