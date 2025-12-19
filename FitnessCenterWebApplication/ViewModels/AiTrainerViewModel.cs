using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Gerekli

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

        public string Gender { get; set; } = "Erkek";

        [Required(ErrorMessage = "Hedefinizi belirtiniz.")]
        public string Goal { get; set; }

        public string? ActivityLevel { get; set; }

        // --- YAPAY ZEKA ÇIKTILARI ---

        // 1. Gemini'nin Metin Cevabı (Diyet/Program)
        public string? AiTextResponse { get; set; }

        // 2. DeepAI'ın Oluşturduğu "Sonraki Halin" Fotoğraf URL'i
        public string? GeneratedImageUrl { get; set; }

        // --- KULLANICININ YÜKLEDİĞİ FOTOĞRAF ---

        [Display(Name = "Vücut Fotoğrafınız (Analiz için)")]
        [Required(ErrorMessage = "Analiz için lütfen bir fotoğraf yükleyin.")]
        public IFormFile? UserImageFile { get; set; }

        // Yüklenen fotoyu ekranda göstermek için Base64 hali
        public string? UserImageBase64 { get; set; }
    }
}