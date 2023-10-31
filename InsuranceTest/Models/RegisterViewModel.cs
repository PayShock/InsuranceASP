using System.ComponentModel.DataAnnotations;  

namespace InsuranceTest.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Jméno")]
        [Required(ErrorMessage = "Vyplňte jméno")]
        public string Name { get; set; } = "";

        [Display(Name = "Příjmení")]
        [Required(ErrorMessage = "Vyplňte příjmení")]
        public string Surname { get; set; } = "";

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vyplňte email")]
        [EmailAddress(ErrorMessage = "Neplatná emailová adresa")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";

        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Vyplňte telefon")]
        public string Phone { get; set; } = "";

        [Display(Name = "Ulice")]
        public string Street { get; set; } = "";

        [Display(Name = "Město")]
        [Required(ErrorMessage = "Vyplňte město")]
        public string City { get; set; } = "";

        [Display(Name = "PSČ")]
        [Required(ErrorMessage = "Vyplňte PSČ")]
        public string Zip { get; set; } = "";

        [Display(Name = "Heslo")]
        [Required(ErrorMessage = "Zadejte heslo")]
        [StringLength(100, ErrorMessage = "Heslo musí být alespoň 8 znaků dlouhé", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Potvrzení hesla")]
        [Required(ErrorMessage = "Zadejte heslo")]
        [Compare("Password", ErrorMessage = "Zadaná hesla se neshodují")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
