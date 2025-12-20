using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.Models.ViewModels
{
    public class AiTrainerViewModel
    {
        [Required(ErrorMessage = "Yaş alanı zorunludur.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Kilo alanı zorunludur.")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Boy alanı zorunludur (cm).")]
        public double Height { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçiniz.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Hedefinizi belirtiniz.")]
        public string Goal { get; set; } 

        public string? ActivityLevel { get; set; } 

        public string? AiResponse { get; set; }
    }
}