using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class DebitPayInfo
{
    public int Id { get; set; }

    public int? DebitPayNo { get; set; }

    public DateOnly? DebitPayDate { get; set; }

    public decimal? DebitPayQty { get; set; }

    public int? DeleteFlag { get; set; }

    public int? ViolationId { get; set; }

    public int? UserId { get; set; }

    public int? UserRecievedId { get; set; }

    public DateOnly? UserRecievedDate { get; set; }

    public string? Hent { get; set; }

    public string? DeleteReson { get; set; }

    public int? DebitInfoId { get; set; }

    public virtual DebitInfo? DebitInfo { get; set; }

    public virtual PasswordDatum? User { get; set; }
    public int? UserRecievedNo { get; set; }


    public virtual PasswordDatum? UserRecieved { get; set; }

  public virtual ViolationInfo? ViolationInfo { get; set; }

}
