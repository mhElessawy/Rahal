using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class PasswordDatum
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? UserFullName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserMobile { get; set; }

    public int? DeleteFlag { get; set; }

    public bool UserAdmin { get; set; } = false;

    public int? EmpId { get; set; }

    public string? RecievedPassword { get; set; }

    public bool CompView { get; set; } = false;

    public bool CompSave { get; set; } = false;

    public bool CompDelete { get; set; } = false;

    public bool EmpView { get; set; } = false;

    public bool EmpSave { get; set; } = false;

    public bool EmpDelete { get; set; } = false;

    public bool EmpArchive { get; set; } = false;

    public bool CarView { get; set; } = false;

    public bool CarSave { get; set; } = false;

    public bool Cardelete { get; set; } = false;

    public bool CarArchive { get; set; } = false;

    public bool ContractDailyView { get; set; } = false;

    public bool ContractDailySave { get; set; } = false;

    public bool ContractDailyEnd { get; set; } = false;

    public bool ContractMonthlyView { get; set; } = false;

    public bool ContractChangeCar { get; set; } = false;

    public bool ContractMonthlySave { get; set; } = false;

    public bool ContractMonthlyEnd { get; set; } = false;

    public bool ContractArchive { get; set; } = false;

    public bool PayView { get; set; } = false;

    public bool PaySave { get; set; } = false;

    public bool PayDelete { get; set; } = false;

    public bool DebitView { get; set; } = false;

    public bool DebitSave { get; set; } = false;
    public bool DebitUpdate { get; set; } = false;

    public bool DebitDelete { get; set; } = false;

    public bool DebitPay { get; set; } = false;

    public bool VacationView { get; set; } = false;

    public bool DebitLatePay { get; set; } = false;
   
    public bool ContractChangeRent { get; set; } = false;
    public bool RecievedMoney { get; set; } = false;
   
    public bool PurshaseView { get; set; } = false;
    public bool PurshaseSave { get; set; } = false;

    public bool PurshaseDelete { get; set; } = false;
    public bool PurshaseShowAll { get; set; } = false;

    public  bool PurshaseUpdate {get;set;} = false;

    public bool EmpTakeMoney { get; set; } = false;

    public bool ViolationView { get; set; } = false;

    public bool ViolationSave { get; set; } = false;

    public bool ViolationUpdate { get; set; } = false;

    public bool ViolationDelete { get; set; } = false;

    public bool CompDebitView { get; set; } = false;    
    public bool CompDebitSave { get; set; } = false;    
    public bool CompDebitDelete { get; set; } = false;
    public bool CompDebitUpdate { get; set; } = false;

    public string? CompanyData { get; set; }
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<CarInfo> CarInfos { get; set; } = new List<CarInfo>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<CreditBill> CreditBills { get; set; } = new List<CreditBill>();

    public virtual ICollection<DebitInfo> DebitInfos { get; set; } = new List<DebitInfo>();

    public virtual ICollection<DebitPayInfo> DebitPayInfoUserRecieveds { get; set; } = new List<DebitPayInfo>();

    public virtual ICollection<DebitPayInfo> DebitPayInfoUsers { get; set; } = new List<DebitPayInfo>();

    public virtual ICollection<EmployeeInfo> EmployeeInfos { get; set; } = new List<EmployeeInfo>();

    public virtual ICollection<UserCompanyNotAppear> UserCompanyNotAppears { get; set; } = new List<UserCompanyNotAppear>();
    public virtual ICollection<ViolationInfo> ViolationInfos { get; set; } = new List<ViolationInfo>();

    public virtual ICollection<EmployeeTakeMoney> EmployeeTakeMoneyTakeUser { get; set; } = new List<EmployeeTakeMoney>();
    public virtual ICollection<EmployeeTakeMoney> EmployeeTakeMoneyUser { get; set; } = new List<EmployeeTakeMoney>();

    public virtual ICollection<CompanyDebit> CompanyDebitsUser { get; set; } = new List<CompanyDebit>();
    public virtual ICollection<CompanyDebit> CompanyDebitsUserRecieved { get; set; } = new List<CompanyDebit>();

    public virtual ICollection<CompanyDebitDetails> CompanyDebitsDetailsUser { get; set; } = new List<CompanyDebitDetails>();
    public virtual ICollection<CompanyDebitDetails> CompanyDebitsDetailsUserRecieved { get; set; } = new List<CompanyDebitDetails>();
    public virtual ICollection<EmployeeSalary> EmployeeSalarys { get; set; } = new List<EmployeeSalary>();

}
