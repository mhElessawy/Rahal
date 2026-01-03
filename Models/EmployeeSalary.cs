namespace RahalWeb.Models
{
    public class EmployeeSalary
    {
        public int Id { get; set; }
        public int EmpId { get; set; }
        public int EmpSalaryYear  { get; set; }
        public int EmpSalaryMonth { get;set; }
        public DateOnly  EntryDate { get; set; }
      
        public decimal EmpSalary { get; set; }
        public decimal  EmpSpecial { get; set; }
        public decimal AddBasicSalary { get; set; }
        public decimal AddFood { get; set; }
        public decimal AddSpecial { get; set; }
        public decimal AddTravel { get; set; }
        public decimal AddRoom { get; set; }
        public decimal DeductPenality { get; set; }
        public decimal DeductAdvance { get; set; }
        public decimal DeductRoom { get; set; }
        public decimal TotalPayed { get; set; }

        public decimal TotalDeduuct { get; set; }
        public decimal Net { get; set; }
        public int Approve { get; set; }
        public int SalaryRecieved { get; set; }
        public int UserId { get; set; }

        public virtual PasswordDatum? User { get; set; }

        public virtual EmployeeInfo? Emp { get; set; }
    }
}
