using System.ComponentModel.DataAnnotations;

namespace FitnessCenterWebApplication.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage ="Email Gereklidir")]
        [EmailAddress]
        public string Email{ get; set; }
    }
}
