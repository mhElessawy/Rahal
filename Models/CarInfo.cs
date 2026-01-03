using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class CarInfo
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    public int? CarCode { get; set; }

    public DateOnly? RegDate { get; set; }

    public int? CarTypeId { get; set; }

    public int? CarKindId { get; set; }

    public int? CarShapeId { get; set; }

    public int? CarModel { get; set; }

    public string? CarNoOfSystemRound { get; set; }

    public string? CarShase { get; set; }

    public string? CarNo { get; set; }

    public string? CarColor { get; set; }

    public DateOnly? CarEndLicense { get; set; }

    public int? DeleteFlag { get; set; }

    public DateOnly? CarReg { get; set; }

    public DateOnly? CarEndDate { get; set; }

    public int? UserId { get; set; }

    public string? CarPaper { get; set; }

    public decimal CarCredit { get; set; }
    public int NoOfCredit { get; set; }


    public virtual ICollection<CarInfoAtt> CarInfoAtts { get; set; } = new List<CarInfoAtt>();

    public virtual Deff? CarKind { get; set; }

    public virtual Deff? CarShape { get; set; }

    public virtual Deff? CarType { get; set; }

    public virtual CompanyInfo? Company { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual PasswordDatum? User { get; set; }
    public virtual ICollection<ViolationInfo> ViolationInfos { get; set; } = new List<ViolationInfo>();

}
