namespace RahalWeb.Models
{
    public class EmployeeTakeMoney
    {
        public int Id { get; set; }
        public int TakeUserId { get; set; } = 0;
        public decimal TakeMoney { get; set; }
        public DateOnly TakeDate { get; set; }
        public int DeleteFlag { get; set; }
        public int UserId {  get; set; }
        public int TakeMoneyNo { get; set; }

        public string? EmpReson { get; set; }

        public virtual PasswordDatum? TakeUser { get; set; }
        public virtual PasswordDatum? User { get; set; }
    }
}
