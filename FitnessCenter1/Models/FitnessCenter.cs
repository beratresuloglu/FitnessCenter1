using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class FitnessCenter
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Şube adı zorunludur")]
        [Display(Name = "Şube adı")]
        [MaxLength(25, ErrorMessage = "Şube adı 50 karakterden fazla olamaz")]
        public String Name { get; set; }


        [Display(Name = "Şube adresi")]
        public String Address { get; set; }


        [Required(ErrorMessage = "Çalışma saatleri zorunludur")]
        [Display(Name = "Çalışma saatleri")]
        public string WorkingHours { get; set; }

    }
}
