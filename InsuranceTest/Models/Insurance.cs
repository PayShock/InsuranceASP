using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Xml.Linq;

namespace InsuranceApp.Models
{
    public class Insurance
    {
        // Id = primary table key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; } = 0;

        [Display(Name = "Typ pojištění")]
        [Required(ErrorMessage = "Vyplňte typ pojištění")]
        public string Type { get; set; } = "";

        [Display(Name = "Pojistná částka")]
        [Required(ErrorMessage = "Vyplňte pojistnou částku")]
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = false)]
        [Range(0, double.MaxValue, ErrorMessage = "Částka nesmí být záporná")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Amount { get; set; }  // ? used for functional Error Message in attribute Required (null) 

        [Display(Name = "Předmět pojištění")]
        [Required(ErrorMessage = "Vyplňte předmět pojištění")]
        public string Subject { get; set; } = "";

        [Display(Name = "Platnost od")]
        [Required(ErrorMessage = "Vyplňte začátek platnosti pojištění")]
        [DataType(DataType.Date)]   // DateTime limits only to Date
        public DateTime? DurationSince { get; set; }  // ? used for functional Error Message in attribute Required (null) 

        [Display(Name = "Platnost do")]
        [Required(ErrorMessage = "Vyplňte konec platnosti pojištění")]
        [DataType(DataType.Date)]  // DateTime limits only to Date
        public DateTime? DurationTill { get; set; }  // ? used for functional Error Message in attribute Required (null)

        // Navigation - Relation 1:N between entities Insured <-- Insurance
        [ForeignKey("Insured")]             // Foreign key from entity Insured
        public int InsuredId { get; set; }  // Foreign key to entity Insured

        // Virtual = won´t be in DB table
        public virtual Insured ? Insured { get; set; }  // Link to Insured

        // Navigation - Relation 1:N between entities Insurance <-- InsuredEvent
        // Virtual = won´t be in DB table
        public virtual ICollection<InsuredEvent>? InsuredEvents { get; set; }

        // List of Insurance types 
        public static List<SelectListItem> GetInsuranceTypes()
        {
            var insuranceTypes = new List<SelectListItem>();
            insuranceTypes.Add(new SelectListItem { Text = "Pojištění osob", Value = "Pojištění osob", Selected = true });
            insuranceTypes.Add(new SelectListItem { Text = "Pojištění majetku", Value = "Pojištění majetku" });
            insuranceTypes.Add(new SelectListItem { Text = "Pojištění vozidel", Value = "Pojištění vozidel" });
            insuranceTypes.Add(new SelectListItem { Text = "Životní pojištění", Value = "Životní pojištění" });
            return insuranceTypes;
        }
    }
}
