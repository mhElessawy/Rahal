using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models
{
    public class DeffInformation
    {
        [Key]
        public int Id { get; set; }
        public double? DebitPayLateDay { get; set; }
        [ForeignKey("DeffPayLate")]  // This points to the navigation property
        public int? DebitPayLatId { get; set; }  // Renamed for clarity
        public virtual Deff? DeffPayLate { get; set; }  // Renamed for consistency

    }
}
