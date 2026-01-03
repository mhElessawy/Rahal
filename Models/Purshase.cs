using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class Purshase
{
    public int Id { get; set; }

    public int? PurshaseNo { get; set; }

    public int? PurshaseId { get; set; }

    public decimal? PurshasePayed { get; set; }

    public DateOnly? PurshaseDate { get; set; }

    public string? PurshaseDescription { get; set; }

    public string? PurshaseBillNo { get; set; }

    public int? CarId { get; set; }

    public int? EmpId { get; set; }

    public int? CompId { get; set; }

    public int? DeleteFlag { get; set; }

    public int? UserId { get; set; }

    public virtual Deff? PurshaseNavigation { get; set; }
}
