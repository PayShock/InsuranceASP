using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace InsuranceApp.Models
{
    //Insured person
    public class Insured
    {
        // Id = primary table key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; } = 0;
        
        [Display(Name = "Jméno")]
        [Required(ErrorMessage = "Vyplňte jméno")]
        [StringLength(60, ErrorMessage = "Jméno je příliš dlouhé")]
        public string Name { get; set; } = "";
        
        [Display(Name = "Příjmení")]
        [Required(ErrorMessage = "Vyplňte příjmení")]
        [StringLength(60, ErrorMessage = "Příjmení je příliš dlouhé")]
        public string Surname { get; set; } = "";

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vyplňte email"), MinLength(1), MaxLength(70)]
        //[EmailAddress(ErrorMessage = "Neplatná emailová adresa")]   // Server validation
        [DataType(DataType.EmailAddress)]                           // Server validation
        public string Email { get; set; } = "";
        
        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Vyplňte telefon")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Zadejte, prposím, validní telefonní číslo")]
        public string Phone { get; set; } = "";
        
        [Display(Name = "Ulice a č.p.")]
        [Required(ErrorMessage = "Vyplňte ulici a č.p.")]
        public string Street { get; set; } = "";
        
        [Display(Name = "Město")]
        [Required(ErrorMessage = "Vyplňte město")]
        public string City { get; set; } = "";
        
        [Display(Name = "PSČ")]
        [Required(ErrorMessage = "Vyplňte PSČ")]
        [StringLength(6, MinimumLength = 5)]
        [RegularExpression("(^\\d{5}(-\\d{4})?$)|(^[ABCEGHJKLMNPRSTVXY]{1}\\d{1}[A-Z]{1} *\\d{1}[A-Z]{1}\\d{1}$)", ErrorMessage = "Zadejte, prosím, validní PSČ")]
        public string Zip { get; set; } = "";

        // Navigation - relaton 1:N between entities Insured <-- Insurance
        // Virtual = won´t be in DB table
        public virtual ICollection<Insurance>? Insurances { get; set; }

        [NotMapped]  
        [Required(ErrorMessage = "Zadejte heslo")]
        [StringLength(100, ErrorMessage = "Heslo musí být alespoň 8 znaků dlouhé", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public virtual string Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Zadejte heslo")]
        [Compare("Password", ErrorMessage = "Zadaná hesla se neshodují")]
        [DataType(DataType.Password)]
        [Display(Name = "Potvrzení hesla")]
        public virtual string ConfirmPassword { get; set; }
    }
}
