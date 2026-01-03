using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class ContractDetail
{
    public int Id { get; set; }

    public int? ContractId { get; set; }

    public DateOnly? DailyCreditDate { get; set; }

    public decimal? DailyCredit { get; set; }

    public int? BillId { get; set; }

    public DateOnly? PayedDate { get; set; }

    public int? Status { get; set; }

    public decimal? CarCredit { get; set; }

    public int? DeleteFlag { get; set; }

    public virtual Bill? Bill { get; set; }

    public virtual Contract? Contract { get; set; }
}
