using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class EmployeeInfo
{
    public int Id { get; set; }

    public int? EmpCode { get; set; }

    public string? FirstNameAr { get; set; }

    public string? SecondNameAr { get; set; }

    public string? ThirdNameAr { get; set; }

    public string? ForthNameAr { get; set; }

    public string? LastNameAr { get; set; }

    public string? FirstNameEn { get; set; }

    public string? SecondNameEn { get; set; }

    public string? ThirdNameEn { get; set; }

    public string? ForthNameEn { get; set; }

    public string? LastNameEn { get; set; }

    public string? CivilId { get; set; }

    public string? ResNo { get; set; }

    public DateOnly? ResEndDate { get; set; }

    public int? CompanyId { get; set; }

    public int? JobTitleId { get; set; }

    public int? NationalityId { get; set; }

    public int? RelationId { get; set; }

    public DateOnly? EmpBirthDate { get; set; }

    public string? PassportNo { get; set; }

    public DateOnly? PassportStartDate { get; set; }

    public DateOnly? PassportEndDate { get; set; }

    public DateOnly? RegDate { get; set; }

    public decimal? Salary { get; set; }

    public string? EmpPic { get; set; }

    public string? EmpAddress { get; set; }

    public int? Gender { get; set; }

    public string? TelNo { get; set; }

    public string? MobiileNo { get; set; }

    public string? AutoAddressNo { get; set; }

    public string? FullNameAr { get; set; }

    public string? FullNameEn { get; set; }

    public int? DeleteFlag { get; set; }

    public int? LocationId { get; set; }

    public DateOnly? StartLicense { get; set; }

    public DateOnly? EndLicense { get; set; }

    public DateOnly? StartPerm { get; set; }

    public DateOnly? EndPerm { get; set; }

    public DateOnly? CivilIdendDate { get; set; }

    public int? UserId { get; set; }

    public bool EmpDepMang { get; set; } = false;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual CompanyInfo? Company { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<CreditBill> CreditBills { get; set; } = new List<CreditBill>();

    public virtual ICollection<DebitInfo> DebitInfos { get; set; } = new List<DebitInfo>();

    public virtual ICollection<EmployeeInfoAtt> EmployeeInfoAtts { get; set; } = new List<EmployeeInfoAtt>();
    public virtual ICollection<EmployeeSalary> EmployeeSalarys { get; set; } = new List<EmployeeSalary>();


    public virtual Deff? JobTitle { get; set; }

    public virtual Deff? Location { get; set; }

    public virtual Deff? Nationality { get; set; }

    public virtual Deff? Relation { get; set; }

    public virtual PasswordDatum? User { get; set; }

    public virtual ICollection<Vacation> Vacations { get; set; } = new List<Vacation>();
    public virtual ICollection<ViolationInfo> ViolationInfos { get; set; } = new List<ViolationInfo>();
    public virtual ICollection<CompanyDebit> CompanyDebits { get; set; } = new List<CompanyDebit>();

}
