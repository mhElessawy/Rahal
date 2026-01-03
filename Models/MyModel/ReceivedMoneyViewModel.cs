namespace RahalWeb.Models.MyModel
{
    public class ReceivedMoneyViewModel
    {
        public required IEnumerable<DebitPayInfo> DebitPayInfos { get; set; }
        public required IEnumerable<Bill> Bills { get; set; }
       
        public required IEnumerable<CompanyDebitDetails> companyDebitDetails { get; set; }
        public decimal TotalBillPayed { get; set; }
    }
}
