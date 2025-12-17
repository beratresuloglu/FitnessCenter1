using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email Gereklidir")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre kısmı boş bırakılamaz")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Onay Şifresi kısmı boş bırakılamaz")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi onayla")]
        [Compare("NewPassword", ErrorMessage = "Şifre ve Onay Şifresi eşleşmiyor.")] // Buraya dikkat: NewPassword ile karşılaştırmalı
        public string ConfirmNewPassword { get; set; } // İsmi View ile aynı yaptık (ConfirmNewPassword)
    }
}
