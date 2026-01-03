namespace RahalWeb.Models.MyModel
{
    public class ContractDetailsSumation
    {
        public int EmployeeId { get; set; }
        public int EmpCode {  get; set; }

        public string? MobileNo {  get; set; }
        public string? EmployeeName { get; set; }
        public decimal TotalDailyCredit { get; set; }
        public decimal TotalCarCredit { get; set; }
    }
    public class CarInfoWithCreditsViewModel
    {
        public CarInfo Car { get; set; }
        public decimal PayedCredit { get; set; }
        public decimal RemainingCredit { get; set; }
    }
}
