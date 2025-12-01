namespace FitnessCenter1.Models
{
    public class Trainer
    {
        public int TrainerID{ get; set; }
        public int UserID { get; set; } // atrenör de bir kullanıcıdır, admin değildir.
        public string TrainerExpertise { get; set; } // uzmanlık alanı
    }
}
