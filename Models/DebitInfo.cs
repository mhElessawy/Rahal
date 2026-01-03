using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class DebitInfo
{
    public int Id { get; set; }

    public int? DebitNo { get; set; }

    public int? EmpId { get; set; }

    public int? DebitTypeId { get; set; }

    public DateOnly? DebitDate { get; set; }

    public decimal? DebitQty { get; set; }

    public string? DebitDescrp { get; set; }

    public int? DeleteFlag { get; set; }

    public int? UserId { get; set; }

    public int? ViolationId { get; set; }

    public string? DeleteReson { get; set; }

    public decimal? DebitPayed { get; set; }

    public decimal? DebitRemaining { get; set; }

    public virtual ICollection<DebitPayInfo> DebitPayInfos { get; set; } = new List<DebitPayInfo>();

    public virtual Deff? DebitType { get; set; }

    public virtual EmployeeInfo? Emp { get; set; }

    public virtual PasswordDatum? User { get; set; }

    public virtual Deff? Violation { get; set; }

   

}
