using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class ViolationInfo
{
    public int Id { get; set; }

    public int? CarId { get; set; }

    public DateOnly? ViolationDate { get; set; }

    public DateTime? ViolationTime { get; set; }

    public string? ViolationPlace { get; set; }

    public double? ViolationSpeed { get; set; }

    public int? ViolationGuideId { get; set; }

    public decimal? ViolationCost { get; set; }

    public int? ViolationPoint { get; set; }

    public int? TransfereToDebit { get; set; }

    public int? DeleteFlag { get; set; }

    public int? ViolationBookNo { get; set; }

   public int? EmpId { get; set; }

    public string? ViolationNo { get; set; }

    public int? UserId { get; set; }

    public virtual EmployeeInfo? Employee { get; set; }

    public virtual PasswordDatum? User { get; set; }
    public virtual CarInfo? Car { get; set; }

    public virtual Deff? ViolationGuide { get; set; }
    public virtual ICollection<DebitPayInfo> DebitPayInfos { get; set; } = new List<DebitPayInfo>();
}
