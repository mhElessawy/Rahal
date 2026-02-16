namespace RahalWeb.Models
{
    public class DeffEmpTreatment
    {
        public int Id { get; set; }
        public string? DeffCode { get; set; }
        public string? DeffName { get; set; }
        public decimal? Price1 { get; set; }
        public decimal? Price2 { get; set; }
        public decimal? Price3 { get; set; }
        public int? DeleteFlag { get; set; }
    }
}
