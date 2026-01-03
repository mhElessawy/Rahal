using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class Bill
{
    public int Id { get; set; }

    public int? BillNo { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public int? NoOfDays { get; set; }

    public DateOnly? BillDate { get; set; }

    public TimeOnly? BillTime { get; set; }

    public decimal? BillPayed { get; set; }

    public int? LateDays { get; set; }

    public int? ContractId { get; set; }

    public int? DeleteFlag { get; set; }

    public int? UserRecievedId { get; set; }

    public DateOnly? UserRecievedDate { get; set; }

    public int? BankIntNo { get; set; }

    public string? BankBillNo { get; set; }

    public DateOnly? BankDate { get; set; }

    public string? BillHent { get; set; }

    public string? DeleteReson { get; set; }

    public int? UserId { get; set; }

    public int? UserRecievedNo { get; set; }

    public virtual Deff? BankIntNoNavigation { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<ContractDetail> ContractDetails { get; set; } = new List<ContractDetail>();

    public virtual EmployeeInfo? Employee { get; set; }

    public virtual PasswordDatum? User { get; set; }
}
