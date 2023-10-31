using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace InsuranceTest.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Zadejte email")]                      
        [EmailAddress(ErrorMessage = "Neplatná emailová adresa")]      
        [DataType(DataType.EmailAddress)]                                            
        public string Email { get; set; }

        [Display(Name = "Heslo")]
        [Required(ErrorMessage = "Zadejte heslo")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Pamatuj si mě")]
        public bool RememberMe { get; set; }
    }
}
