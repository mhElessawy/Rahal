using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models;

public partial class CompanyInfo
{
    public int Id { get; set; }

    public int? CompCode { get; set; }

    public string? CompNameAr { get; set; }

    public string? CompNameEn { get; set; }
    [NotMapped] // Prevent this property from being mapped to the database
    public IFormFile? ImageFile1 { get; set; }

    public string? CompLogo { get; set; }

    public string? OwnerName1 { get; set; }

    public string? OwnerCivilId1 { get; set; }

    public string? OwnerName2 { get; set; }

    public string? OwnerCivilId2 { get; set; }

    public string? OwnerName3 { get; set; }

    public string? OwnerCivilId3 { get; set; }

    public string? CompOwnerNumber { get; set; }

    public string? CompLicenseNo { get; set; }

    public string? CompFileNo { get; set; }

    public int? LocationId { get; set; }

    public int? CityId { get; set; }

    public string? Address { get; set; }

    public string? Tel1 { get; set; }

    public string? Tel2 { get; set; }

    public DateOnly? CompReleaseDate { get; set; }

    public DateOnly? CompEndDate { get; set; }

    public string? AddressNo { get; set; }

    public DateOnly? AcceptNewerDate { get; set; }

    public DateOnly? RegDate { get; set; }

    public int? DeleteFlag { get; set; }

    public string? CompAutoNo { get; set; }

    public int? CompActivateId { get; set; }

    public string? CompPaper { get; set; }

    public virtual ICollection<CarInfo> CarInfos { get; set; } = new List<CarInfo>();

    public virtual Deff? City { get; set; }

    public virtual Deff? CompActivate { get; set; }

    public virtual ICollection<CompanyInfoAtt> CompanyInfoAtts { get; set; } = new List<CompanyInfoAtt>();

    public virtual ICollection<EmployeeInfo> EmployeeInfos { get; set; } = new List<EmployeeInfo>();

    public virtual Deff? Location { get; set; }

    public virtual ICollection<UserCompanyNotAppear> UserCompanyNotAppears { get; set; } = new List<UserCompanyNotAppear>();
}
