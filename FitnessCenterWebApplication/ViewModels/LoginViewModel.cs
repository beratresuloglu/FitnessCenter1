using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email kısmı boş bırakılamaz")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre kısmı boş bırakılamaz")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
