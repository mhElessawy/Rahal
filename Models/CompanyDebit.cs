using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models
{
    public class CompanyDebit
    {
        public int Id { get; set; }
        public int CompDebitNo { get; set; }
        public decimal DebitQty { get; set; }
        public DateOnly DebitDate { get; set; }
        public decimal PayedQty { get; set; }
        public decimal ReminderQty { get; set; }
        public int UserId { get; set; }
        public int DeletFlag { get; set; }
        public string? DebitReson { get; set; }
        public int EmpId { get; set; }
        public string? OtherData { get; set; }
        public int? UserRecievedId { get; set; }

        [ForeignKey("UserId")]
        public virtual PasswordDatum? UserInfo { get; set; }

        [ForeignKey("EmpId")]
        public virtual EmployeeInfo? Employee { get; set; }

        [ForeignKey("UserRecievedId")]
        public virtual PasswordDatum? UserInfoRecieve { get; set; }

        public virtual ICollection<CompanyDebitDetails> CompanyDebitsDetails { get; set; } = new List<CompanyDebitDetails>();
      // public virtual ICollection<CompanyDebitDetails> CompanyDebitsDetails { get; set; }

    }
}
