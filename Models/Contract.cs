using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class Contract
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public int? CarId { get; set; }

    public string? ContractNo { get; set; }

    public DateOnly? ContractDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? NoOfDays { get; set; }

    public decimal? DailyCredit { get; set; }

    public int? DeleteFlag { get; set; }

    public int? Status { get; set; }

    public decimal? TotalCost { get; set; }

    public DateOnly? ContractEndDate { get; set; }

    public string? ContractEndReson { get; set; }

    public int? ContractType { get; set; }

    public int? UserId { get; set; }

    public DateOnly? CreditStartDate { get; set; }

    public DateOnly? CreditEndDate { get; set; }

    public int? CreditNoOfMonth { get; set; }

    public decimal? CreditMonthPay { get; set; }

    public decimal? CreditTotalCost { get; set; }

    public bool HaveVacation { get; set; } = false;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual CarInfo? Car { get; set; }

    public virtual ICollection<CreditBill> CreditBills { get; set; } = new List<CreditBill>();

    public virtual EmployeeInfo? Employee { get; set; }

    public virtual PasswordDatum? User { get; set; }
    public virtual ICollection<ContractDetail> ContractDetails { get; set; } = new List<ContractDetail>();
}
