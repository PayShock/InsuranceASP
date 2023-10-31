using System.ComponentModel.DataAnnotations;  

namespace InsuranceTest.Models
{
     public class ChangePasswordViewModel
     {
        [Display(Name = "Aktuální heslo")]
        [Required(ErrorMessage = "Aktuální heslo je povinné")]               
        [DataType(DataType.Password)]                                             
        public string OldPassword { get; set; }

        [Display(Name = "Nové heslo")]
        [Required(ErrorMessage = "Nové heslo je povinné")]
        [StringLength(100, ErrorMessage = "Heslo musí být minimálně 8 znaků dlouhé", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Potvrzení nového hesla")]
        [Compare("NewPassword", ErrorMessage = "Zadaná hesla se neshodují")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
     }
}
