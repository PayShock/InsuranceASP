using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace InsuranceApp.Models
{
    public class InsuredEvent
    {
        // Id = primary table key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; } = 0;

        [Display(Name = "Popis události")]
        [Required(ErrorMessage = "Vyplňte popis události")]
        public string Description { get; set; } = "";

        [Display(Name = "Datum události")]
        [Required(ErrorMessage = "Vyplňte datum pojistné události")]
        [DataType(DataType.Date)]   // DateTime limits only to Date
        public DateTime? Date { get; set; }  // ? used for functional Error Message in attribute Required (null)

        [Display(Name = "Stav pojistné události")]
        [Required(ErrorMessage = "Vyberte stav pojistné události")]
        public string Status { get; set; } = "";

        [Display(Name = "Výše plnění")]
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = false)]
        [Range(0, double.MaxValue, ErrorMessage = "Cena nesmí být záporná")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Amount { get; set; }  // ? used for functional Error Message in attribute Required (null)


        // Navigation - Relation 1:N between entities Insurance <-- InsuredEvent
        [ForeignKey("Insurance")]             // Foreign key from entity Insured
        public int InsuranceId { get; set; }  // Foreign key to entity InsuredEvent

        // Virtual = won´t be in DB table
        public virtual Insurance? Insurance { get; set; }  // Link to Insurance

        // List of Insured Events statuses
        public static List<SelectListItem> GetStatusTypes()
        {
            var statusTypes = new List<SelectListItem>();
            statusTypes.Add(new SelectListItem { Text = "Oznámeno", Value = "Oznámeno", Selected = true });
            statusTypes.Add(new SelectListItem { Text = "Vyřizuje se", Value = "Vyřizuje se" });
            statusTypes.Add(new SelectListItem { Text = "Vyplaceno", Value = "Vyplaceno" });
            statusTypes.Add(new SelectListItem { Text = "Zamítnuto", Value = "Zamítnuto" });
            return statusTypes;
        }

    }
}
