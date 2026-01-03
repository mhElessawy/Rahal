using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class Vacation
{
    public int Id { get; set; }

    public int? EmpId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public int? NoOfDays { get; set; }

    public int? VacationPayed { get; set; }

    public int? DeleteFlag { get; set; }

    public int? VacationStatus { get; set; }

    public virtual EmployeeInfo? Emp { get; set; }
}
