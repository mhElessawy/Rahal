namespace RahalWeb.Models
{
    public class EmpTreatment
    {
        public int Id { get; set; }
        public int? EmpId { get; set; }
        public int? DeffEmpTreatmentId { get; set; }
        public int? TreatmentNo { get; set; }
        public DateTime? Date { get; set; }
        public string? TreatmentDetails { get; set; }
        public decimal? TreatmentExtraMoney { get; set; }
        public decimal? TreatmentTotal { get; set; }
        public int? DeleteFlag { get; set; }
        public int? UserId { get; set; }
        public int? UserRecievedId { get; set; }
        public DateOnly? UserRecievedDate { get; set; }
        public int? UserRecievedNo { get; set; }
        public virtual PasswordDatum? User { get; set; }
        public virtual PasswordDatum? UserRecieved { get; set; }
        public virtual DeffEmpTreatment? DeffEmpTreatment { get; set; }
        public virtual EmployeeInfo? Emp { get; set; }
    }
}
