namespace RahalWeb.Models
{
    public class DeffEmpTreatment
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public int DeffId { get; set; }
        public DateOnly TreatmentDate { get; set; }
        public decimal TreatmentAmount { get; set; }
        public string? Notes { get; set; }
        public int DeleteFlag { get; set; }
        public int UserId { get; set; }
        public int TreatmentNo { get; set; }

        public virtual EmployeeInfo? Employee { get; set; }
        public virtual Deff? DeffTreatment { get; set; }
        public virtual PasswordDatum? User { get; set; }
    }
}
