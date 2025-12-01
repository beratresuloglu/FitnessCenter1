using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class Admin
    {
        public int AdminID { get; set; }

        [Required(ErrorMessage = "Kullanıcı Adı Zorunlu")]
        [Display(Name = "Kullanıcı Adı")]
        [MaxLength(25, ErrorMessage = "Kullanıcı Adı Maksimum 25 Karakter Olabilir")]
        public string AdminUserName { get; set; }

        [Required(ErrorMessage ="Şifre Zorunlu")]
        [Display(Name = "Şifre")]
        [MinLength(4, ErrorMessage = "Şifre 4 veya daha fazla karakter olmalı")]
        public string AdminPassword { get; set; }
    }
}
