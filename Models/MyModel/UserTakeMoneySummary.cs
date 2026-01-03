namespace RahalWeb.Models.MyModel
{
    public class UserTakeMoneySummary
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalTakeMoney { get; set; }
        public decimal TotalPurshase { get; set; }
        public decimal NetAmount => TotalTakeMoney - TotalPurshase;
    }
}
