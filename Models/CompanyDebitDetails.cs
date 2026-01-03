using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models
{
    public class CompanyDebitDetails
    {
        public  int Id { get; set; }
        public int? CompDebitId { get; set; }
        public int CompDebitDetailsNo { get; set; }
        public decimal? CompDebitPayed { get; set; }
        public DateOnly? CompDebitDate { get; set; }

        public int? CompDebitType { get; set; }   // 1  debit  2  payed
        public int? UserId { get; set; }
        public int? UserRecievedId { get; set; }
        public DateOnly? UserRecievedDate { get; set; }

        public int? UserRecievedNo { get; set; }

        [ForeignKey("UserId")]
        public virtual PasswordDatum? UserInfo { get; set; }

        [ForeignKey("CompDebitId")]
        public virtual CompanyDebit? CompanyDebits { get; set; }

        [ForeignKey("UserRecievedId")]
        public virtual PasswordDatum? UserInfoRecieve { get; set; }
    }
}
