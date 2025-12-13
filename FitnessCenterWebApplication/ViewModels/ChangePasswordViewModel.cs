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
        [Display(Name ="Yeni Şifre")]
        [Compare("ConfirmPassword", ErrorMessage = "Şifre ve Onay Şifresi eşleşmiyor.")]
        public string NewPassword { get; set; }


        [Display(Name = "Şifreyi onayla")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Onay Şifresi kısmı boş bırakılamaz")]
        public bool ConfirmPassword { get; set; }
    }
}
