using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class Deff
{
    public int Id { get; set; }

    public string? DeffName { get; set; }

    public int? DeffType { get; set; }

    public string? DeffCode { get; set; }

    public int? DeffParent { get; set; }

    public int? DeleteFlag { get; set; }

    public string? DeffNameEng { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<CarInfo> CarInfoCarKinds { get; set; } = new List<CarInfo>();

    public virtual ICollection<CarInfo> CarInfoCarShapes { get; set; } = new List<CarInfo>();

    public virtual ICollection<CarInfo> CarInfoCarTypes { get; set; } = new List<CarInfo>();

    public virtual ICollection<CompanyInfo> CompanyInfoCities { get; set; } = new List<CompanyInfo>();

    public virtual ICollection<CompanyInfo> CompanyInfoCompActivates { get; set; } = new List<CompanyInfo>();

    public virtual ICollection<CompanyInfo> CompanyInfoLocations { get; set; } = new List<CompanyInfo>();

    public virtual ICollection<CreditBill> CreditBills { get; set; } = new List<CreditBill>();

    public virtual ICollection<DebitInfo> DebitInfoDebitTypes { get; set; } = new List<DebitInfo>();

    public virtual ICollection<DebitInfo> DebitInfoViolations { get; set; } = new List<DebitInfo>();

    

    public virtual DeffType? DeffTypeNavigation { get; set; }

    public virtual ICollection<EmployeeInfo> EmployeeInfoJobTitles { get; set; } = new List<EmployeeInfo>();

    public virtual ICollection<EmployeeInfo> EmployeeInfoLocations { get; set; } = new List<EmployeeInfo>();

    public virtual ICollection<EmployeeInfo> EmployeeInfoNationalities { get; set; } = new List<EmployeeInfo>();

    public virtual ICollection<EmployeeInfo> EmployeeInfoRelations { get; set; } = new List<EmployeeInfo>();

    public virtual ICollection<Purshase> Purshases { get; set; } = new List<Purshase>();

    public virtual DeffInformation? DeffInformation { get; set; }
    public virtual ICollection<ViolationInfo> ViolationInfos { get; set; } = new List<ViolationInfo>();
    public virtual ICollection<DeffEmpTreatment> DeffEmpTreatments { get; set; } = new List<DeffEmpTreatment>();

}
