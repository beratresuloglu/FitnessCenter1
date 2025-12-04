using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FitnessCenter1.Models
{
    [Table("Members")]
    public class Member
    {
        [Key]
        public int MemberID { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        [MaxLength(25, ErrorMessage = "Ad 25 karakterden fazla olamaz")]
        public String MemberName { get; set; }


        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        [MaxLength(25, ErrorMessage = "Soyad 25 karakterden fazla olamaz")]
        public String MemberSurname { get; set; }


        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [Display(Name = "Kullanıcı adı")]
        [MaxLength(25, ErrorMessage = "Kullanıcı adı 25 karakterden fazla olamaz")]
        public String MemberUserName { get; set; }


        [Required(ErrorMessage = "E-mail zorunludur")]
        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-mail formatı giriniz")]
        public String MemberEmail { get; set; }


        [Required(ErrorMessage = "Şifre zorunludur")]
        [Display(Name = "Şifre")]
        [MinLength(4, ErrorMessage = "Şifre 4 karakter veya daha uzun olmak zorunda")]
        public String MemberPassword { get; set; }
    }
}
