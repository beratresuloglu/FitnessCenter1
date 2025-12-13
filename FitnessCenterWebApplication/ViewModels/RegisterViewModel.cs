using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad kısmı boş bırakılamaz")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Email kısmı boş bırakılamaz")]
        [EmailAddress]
        public String Email { get; set; }
        [Required(ErrorMessage = "Şifre kısmı boş bırakılamaz")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Şifre ve Onay Şifresi eşleşmiyor.")]
        public String Password { get; set; }
        [Required(ErrorMessage = "Onay Şifresi kısmı boş bırakılamaz")]
        [DataType(DataType.Password)]
        [Display(Name = "Onay Şifresi")]
        public String ConfirmPassword { get; set; }
    }
}
