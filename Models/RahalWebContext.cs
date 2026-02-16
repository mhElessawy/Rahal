using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RahalWeb.Models;

public partial class RahalWebContext : DbContext
{
    public RahalWebContext()
    {
    }

    public RahalWebContext(DbContextOptions<RahalWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<CarInfo> CarInfos { get; set; }

    public virtual DbSet<CarInfoAtt> CarInfoAtts { get; set; }

    public virtual DbSet<CompanyInfo> CompanyInfos { get; set; }

    public virtual DbSet<CompanyInfoAtt> CompanyInfoAtts { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

  
    public virtual DbSet<ContractDetail> ContractDetails { get; set; }

    public virtual DbSet<CreditBill> CreditBills { get; set; }

  

    public virtual DbSet<DebitInfo> DebitInfos { get; set; }

    public virtual DbSet<DebitPayInfo> DebitPayInfos { get; set; }

    public virtual DbSet<Deff> Deffs { get; set; }

   
    public virtual DbSet<DeffType> DeffTypes { get; set; }

   
 
    public virtual DbSet<EmployeeInfo> EmployeeInfos { get; set; }

    public virtual DbSet<EmployeeInfoAtt> EmployeeInfoAtts { get; set; }
    public virtual DbSet<EmployeeSalary> EmployeeSalarys { get; set; }

    public virtual DbSet<PasswordDatum> PasswordData { get; set; }
    public virtual DbSet<Purshase> Purshases { get; set; }
    public virtual DbSet<UserCompanyNotAppear> UserCompanyNotAppears { get; set; }
    public virtual DbSet<Vacation> Vacations { get; set; }

    public virtual DbSet<ViolationInfo> ViolationInfos { get; set; }

    public virtual DbSet<EmployeeTakeMoney> EmployeeTakeMoney { get; set; }

    public virtual DbSet<CompanyDebit> CompanyDebits { get; set; }
    public virtual DbSet<CompanyDebitDetails> CompanyDebitDetails { get; set; }
    public virtual DbSet<DeffEmpTreatment> DeffEmpTreatments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bill");

            entity.Property(e => e.BankBillNo).HasMaxLength(50);
            entity.Property(e => e.BankIntNo).HasDefaultValue(0);
            entity.Property(e => e.BillHent)
                .HasMaxLength(1000)
                .HasDefaultValue("no");
            entity.Property(e => e.BillPayed).HasColumnType("money");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.DeleteReson).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserRecievedId).HasDefaultValue(0);

            entity.HasOne(d => d.BankIntNoNavigation).WithMany(p => p.Bills)
                .HasForeignKey(d => d.BankIntNo)
                .HasConstraintName("FK_Bill_Deff");

            entity.HasOne(d => d.Contract).WithMany(p => p.Bills)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_Bill_Contract");

            entity.HasOne(d => d.Employee).WithMany(p => p.Bills)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Bill_EmployeeInfo");

            entity.HasOne(d => d.User).WithMany(p => p.Bills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Bill_PasswordData");
        });

        modelBuilder.Entity<CarInfo>(entity =>
        {
            entity.ToTable("CarInfo");

            entity.Property(e => e.CarColor).HasMaxLength(50);
            entity.Property(e => e.CarNo).HasMaxLength(50);
            entity.Property(e => e.CarNoOfSystemRound).HasMaxLength(50);
            entity.Property(e => e.CarShase).HasMaxLength(50);
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);

            entity.HasOne(d => d.CarKind).WithMany(p => p.CarInfoCarKinds)
                .HasForeignKey(d => d.CarKindId)
                .HasConstraintName("FK_CarInfo_Deff3");

            entity.HasOne(d => d.CarShape).WithMany(p => p.CarInfoCarShapes)
                .HasForeignKey(d => d.CarShapeId)
                .HasConstraintName("FK_CarInfo_Deff1");

            entity.HasOne(d => d.CarType).WithMany(p => p.CarInfoCarTypes)
                .HasForeignKey(d => d.CarTypeId)
                .HasConstraintName("FK_CarInfo_Deff");

            entity.HasOne(d => d.Company).WithMany(p => p.CarInfos)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_CarInfo_CompanyInfo");

            entity.HasOne(d => d.User).WithMany(p => p.CarInfos)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CarInfo_PasswordData");
        });

        modelBuilder.Entity<CarInfoAtt>(entity =>
        {
            entity.ToTable("CarInfoAtt");

            entity.HasOne(d => d.Car).WithMany(p => p.CarInfoAtts)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK_CarInfoAtt_CarInfo");
        });
        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.ToTable("CompanyInfo");

            entity.Property(e => e.AddressNo).HasMaxLength(50);
            entity.Property(e => e.CompActivateId).HasDefaultValue(0);
            entity.Property(e => e.CompAutoNo).HasMaxLength(50);
            entity.Property(e => e.CompFileNo).HasMaxLength(50);
            entity.Property(e => e.CompLicenseNo).HasMaxLength(50);
            entity.Property(e => e.CompLogo).HasMaxLength(50);
            entity.Property(e => e.CompOwnerNumber).HasMaxLength(50);
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.OwnerCivilId1)
                .HasMaxLength(13)
                .HasColumnName("OwnerCivilID1");
            entity.Property(e => e.OwnerCivilId2)
                .HasMaxLength(13)
                .HasColumnName("OwnerCivilID2");
            entity.Property(e => e.OwnerCivilId3)
                .HasMaxLength(13)
                .HasColumnName("OwnerCivilID3");
            entity.Property(e => e.OwnerName1).HasMaxLength(500);
            entity.Property(e => e.OwnerName2).HasMaxLength(500);
            entity.Property(e => e.OwnerName3).HasMaxLength(500);
            entity.Property(e => e.Tel1).HasMaxLength(12);
            entity.Property(e => e.Tel2).HasMaxLength(12);

            entity.HasOne(d => d.City).WithMany(p => p.CompanyInfoCities)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_CompanyInfo_Deff1");

            entity.HasOne(d => d.CompActivate).WithMany(p => p.CompanyInfoCompActivates)
                .HasForeignKey(d => d.CompActivateId)
                .HasConstraintName("FK_CompanyInfo_Deff2");

            entity.HasOne(d => d.Location).WithMany(p => p.CompanyInfoLocations)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK_CompanyInfo_Deff");
        });

        modelBuilder.Entity<CompanyInfoAtt>(entity =>
        {
            
            entity.ToTable("CompanyInfoAtt");

            entity.Property(e => e.PathFileData).HasMaxLength(500);
            entity.Property(e => e.TitleData).HasMaxLength(500);
            entity.Ignore(e => e.pdfFile1);
            entity.HasOne(d => d.Comp).WithMany(p => p.CompanyInfoAtts)
                .HasForeignKey(d => d.CompId)
                .HasConstraintName("FK_CompanyInfoAtt_CompanyInfo");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contract");

            entity.Property(e => e.ContractNo).HasMaxLength(50);
            entity.Property(e => e.ContractType).HasDefaultValue(0);
            entity.Property(e => e.CreditMonthPay).HasColumnType("money");
            entity.Property(e => e.CreditTotalCost).HasColumnType("money");
            entity.Property(e => e.DailyCredit).HasColumnType("money");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.TotalCost).HasColumnType("money");

            entity.HasOne(d => d.Car).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK_Contract_CarInfo");

            entity.HasOne(d => d.Employee).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Contract_EmployeeInfo");

            entity.HasOne(d => d.User).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Contract_PasswordData");
        });

        modelBuilder.Entity<ContractDetail>(entity =>
        {
            entity.Property(e => e.CarCredit).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.DailyCredit).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(0);

            entity.HasOne(d => d.Bill).WithMany(p => p.ContractDetails)
                .HasForeignKey(d => d.BillId)
                .HasConstraintName("FK_ContractDetails_Bill");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractDetails)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_ContractDetails_Contract");
        });

        modelBuilder.Entity<CreditBill>(entity =>
        {
            entity.ToTable("CreditBill");

            entity.Property(e => e.BankBillNo).HasMaxLength(50);
            entity.Property(e => e.BankIntNo).HasDefaultValue(0);
            entity.Property(e => e.BillHent)
                .HasMaxLength(1000)
                .HasDefaultValue("no");
            entity.Property(e => e.CreditBillPayed).HasColumnType("money");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.DeleteReson).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserRecievedId).HasDefaultValue(0);

            entity.HasOne(d => d.BankIntNoNavigation).WithMany(p => p.CreditBills)
                .HasForeignKey(d => d.BankIntNo)
                .HasConstraintName("FK_CreditBill_Deff");

            entity.HasOne(d => d.Contract).WithMany(p => p.CreditBills)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK_CreditBill_Contract");

            entity.HasOne(d => d.Employee).WithMany(p => p.CreditBills)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_CreditBill_EmployeeInfo");

            entity.HasOne(d => d.User).WithMany(p => p.CreditBills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CreditBill_PasswordData");
        });
        modelBuilder.Entity<DebitInfo>(entity =>
        {
            entity.ToTable("DebitInfo");

            entity.Property(e => e.DebitDescrp).HasMaxLength(500);
            entity.Property(e => e.DebitPayed)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.DebitQty).HasColumnType("money");
            entity.Property(e => e.DebitRemaining)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.DeleteReson).HasMaxLength(500);
            entity.Property(e => e.UserId).HasDefaultValue(0);
            entity.Property(e => e.ViolationId).HasDefaultValue(0);

            entity.HasOne(d => d.DebitType).WithMany(p => p.DebitInfoDebitTypes)
                .HasForeignKey(d => d.DebitTypeId)
                .HasConstraintName("FK_DebitInfo_Deff2");

            entity.HasOne(d => d.Emp).WithMany(p => p.DebitInfos)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_DebitInfo_EmployeeInfo");

            entity.HasOne(d => d.User).WithMany(p => p.DebitInfos)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_DebitInfo_PasswordData");

            entity.HasOne(d => d.Violation).WithMany(p => p.DebitInfoViolations)
                .HasForeignKey(d => d.ViolationId)
                .HasConstraintName("FK_DebitInfo_Deff3");


        });

        modelBuilder.Entity<DebitPayInfo>(entity =>
        {
            entity.ToTable("DebitPayInfo");

            entity.Property(e => e.DebitInfoId).HasDefaultValue(0);
            entity.Property(e => e.DebitPayQty).HasColumnType("money");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.DeleteReson).HasMaxLength(500);
            entity.Property(e => e.Hent).HasMaxLength(500);
            entity.Property(e => e.UserId).HasDefaultValue(0);
            entity.Property(e => e.UserRecievedId).HasDefaultValue(0);
            entity.Property(e => e.ViolationId).HasDefaultValue(0);

            entity.HasOne(d => d.DebitInfo)
                  .WithMany(p => p.DebitPayInfos)
                  .HasForeignKey(d => d.DebitInfoId)
                  .HasConstraintName("FK_DebitPayInfo_DebitInfo");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.DebitPayInfoUsers)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("FK_DebitPayInfo_PasswordData");

            entity.HasOne(d => d.UserRecieved)
                  .WithMany(p => p.DebitPayInfoUserRecieveds)
                  .HasForeignKey(d => d.UserRecievedId)
                  .HasConstraintName("FK_DebitPayInfo_PasswordData1");

            // CORRECTED: Configure ViolationInfo relationship
            entity.HasOne(d => d.ViolationInfo)
                  .WithMany(v => v.DebitPayInfos)
                  .HasForeignKey(d => d.ViolationId)
                  .HasConstraintName("FK_DebitPayInfo_ViolationInfo")
                  .OnDelete(DeleteBehavior.Restrict); // Add this for consistency
        });

        modelBuilder.Entity<Deff>(entity =>
        {
            entity.ToTable("Deff");

            entity.Property(e => e.DeffCode).HasMaxLength(10);
            entity.Property(e => e.DeffName).HasMaxLength(500);
            entity.Property(e => e.DeffNameEng).HasMaxLength(500);
            entity.Property(e => e.DeffParent).HasDefaultValue(0);
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);

            entity.HasOne(d => d.DeffTypeNavigation).WithMany(p => p.Deffs)
                .HasForeignKey(d => d.DeffType)
                .HasConstraintName("FK_Deff_DeffType");
        });
   
        modelBuilder.Entity<DeffType>(entity =>
        {
            entity.ToTable("DeffType");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

       modelBuilder.Entity<EmployeeInfo>(entity =>
        {
            entity.ToTable("EmployeeInfo");

            entity.Property(e => e.AutoAddressNo).HasMaxLength(50);
            entity.Property(e => e.CivilId)
                .HasMaxLength(13)
                .HasColumnName("CivilID");
            entity.Property(e => e.CivilIdendDate).HasColumnName("CivilIDEndDate");
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.FirstNameAr).HasMaxLength(50);
            entity.Property(e => e.FirstNameEn).HasMaxLength(50);
            entity.Property(e => e.ForthNameAr).HasMaxLength(50);
            entity.Property(e => e.ForthNameEn).HasMaxLength(50);
            entity.Property(e => e.FullNameEn).HasColumnName("FullNameEN");
            entity.Property(e => e.LastNameAr).HasMaxLength(50);
            entity.Property(e => e.LastNameEn).HasMaxLength(50);
            entity.Property(e => e.MobiileNo).HasMaxLength(50);
            entity.Property(e => e.PassportNo).HasMaxLength(50);
            entity.Property(e => e.ResNo).HasMaxLength(50);
            entity.Property(e => e.Salary).HasColumnType("money");
            entity.Property(e => e.SecondNameAr).HasMaxLength(50);
            entity.Property(e => e.SecondNameEn).HasMaxLength(50);
            entity.Property(e => e.TelNo).HasMaxLength(50);
            entity.Property(e => e.ThirdNameAr).HasMaxLength(50);
            entity.Property(e => e.ThirdNameEn).HasMaxLength(50);

            entity.HasOne(d => d.Company).WithMany(p => p.EmployeeInfos)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_EmployeeInfo_CompanyInfo");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.EmployeeInfoJobTitles)
                .HasForeignKey(d => d.JobTitleId)
                .HasConstraintName("FK_EmployeeInfo_Deff");

            entity.HasOne(d => d.Location).WithMany(p => p.EmployeeInfoLocations)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK_EmployeeInfo_Deff3");

            entity.HasOne(d => d.Nationality).WithMany(p => p.EmployeeInfoNationalities)
                .HasForeignKey(d => d.NationalityId)
                .HasConstraintName("FK_EmployeeInfo_Deff1");

            entity.HasOne(d => d.Relation).WithMany(p => p.EmployeeInfoRelations)
                .HasForeignKey(d => d.RelationId)
                .HasConstraintName("FK_EmployeeInfo_Deff2");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeInfos)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_EmployeeInfo_PasswordData");
        });

        modelBuilder.Entity<EmployeeInfoAtt>(entity =>
        {
            entity.ToTable("EmployeeInfoAtt");

            entity.Property(e => e.EmpId).HasDefaultValue(0);

            entity.HasOne(d => d.Emp).WithMany(p => p.EmployeeInfoAtts)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_EmployeeInfoAtt_EmployeeInfo");
        });

        modelBuilder.Entity<EmployeeSalary>(entity =>
        {
            entity.ToTable("EmployeeSalary");

            entity.Property(e => e.EmpId).HasDefaultValue(0);

            entity.HasOne(d => d.Emp).WithMany(p => p.EmployeeSalarys)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_EmployeeSalary_EmployeeInfo");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeSalarys)
              .HasForeignKey(d => d.UserId)
              .HasConstraintName("FK_EmployeeSalary_PasswordData");

        });




        modelBuilder.Entity<PasswordDatum>(entity =>
        {
            entity.Property(e => e.CompDelete).HasDefaultValue(false);
            entity.Property(e => e.CompSave).HasDefaultValue(false);
            entity.Property(e => e.CompView).HasDefaultValue(false);
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.EmpDelete).HasDefaultValue(false);
            entity.Property(e => e.EmpId).HasDefaultValue(0);
            entity.Property(e => e.EmpSave).HasDefaultValue(false);
            entity.Property(e => e.EmpView).HasDefaultValue(false);
            entity.Property(e => e.RecievedPassword).HasMaxLength(50);
            entity.Property(e => e.UserAdmin).HasDefaultValue(false);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Purshase>(entity =>
        {
            entity.ToTable("Purshase");

            entity.Property(e => e.CarId).HasDefaultValue(0);
            entity.Property(e => e.CompId).HasDefaultValue(0);
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.EmpId).HasDefaultValue(0);
            entity.Property(e => e.PurshasePayed).HasColumnType("money");
            entity.Property(e => e.UserId).HasDefaultValue(0);

            entity.HasOne(d => d.PurshaseNavigation).WithMany(p => p.Purshases)
                .HasForeignKey(d => d.PurshaseId)
                .HasConstraintName("FK_Purshase_Deff");
        });

        modelBuilder.Entity<UserCompanyNotAppear>(entity =>
        {
            entity.ToTable("UserCompanyNotAppear");

            entity.HasOne(d => d.Company).WithMany(p => p.UserCompanyNotAppears)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_UserCompanyNotAppear_CompanyInfo");

            entity.HasOne(d => d.User).WithMany(p => p.UserCompanyNotAppears)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserCompanyNotAppear_PasswordData");
        });

        modelBuilder.Entity<Vacation>(entity =>
        {
            entity.ToTable("Vacation");

            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.VacationPayed).HasDefaultValue(0);
            entity.Property(e => e.VacationStatus).HasDefaultValue(0);

            entity.HasOne(d => d.Emp).WithMany(p => p.Vacations)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_Vacation_EmployeeInfo");
        });

        modelBuilder.Entity<CompanyDebit>(entity =>
        {
            entity.ToTable("CompanyDebit");
            // Primary Key
            entity.HasKey(e => e.Id);

            // Properties configuration
            entity.Property(e => e.DebitQty)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PayedQty)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ReminderQty)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DebitDate)
                .IsRequired();

            // Configure relationships with explicit foreign key names



            entity.HasOne(d => d.UserInfo).WithMany(p => p.CompanyDebitsUser)
               .HasForeignKey(d => d.UserId)
               .HasConstraintName("FK_CompanyDebit_UserInfo");

            entity.HasOne(d => d.UserInfoRecieve).WithMany(p => p.CompanyDebitsUserRecieved)
              .HasForeignKey(d => d.UserRecievedId)
              .HasConstraintName("FK_CompanyDebit_UserInfoRecieve");

           
            entity.HasOne(d => d.Employee).WithMany(p => p.CompanyDebits)
               .HasForeignKey(d => d.EmpId)
               .HasConstraintName("FK_CompanyDebit_Employee");


            // Configure the one-to-many relationship with CompanyDebitDetails
            entity.HasMany(cd => cd.CompanyDebitsDetails)
                  .WithOne(cdd => cdd.CompanyDebits)
                  .HasForeignKey(cdd => cdd.CompDebitId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_CompanyDebitDetails_CompanyDebit");

            // Indexes for better performance
            entity.HasIndex(e => e.CompDebitNo);
            entity.HasIndex(e => e.DebitDate);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EmpId);
        });

        modelBuilder.Entity<CompanyDebitDetails>(entity =>
        {
            entity.ToTable("CompanyDebitDetails");

            // Primary Key
            entity.HasKey(e => e.Id);

            // Properties configuration
            entity.Property(e => e.CompDebitPayed)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.CompDebitDate)
                .IsRequired();

            entity.Property(e => e.UserRecievedDate)
                .IsRequired();

            // Foreign Key relationships - REMOVE HasPrincipalKey
            entity.HasOne(e => e.CompanyDebits)
                  .WithMany(cd => cd.CompanyDebitsDetails)
                  .HasForeignKey(e => e.CompDebitId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_CompanyDebitDetails_CompanyDebit");


            entity.HasOne(d => d.UserInfo).WithMany(p => p.CompanyDebitsDetailsUser)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CompanyDebitDetails_UserInfo");

            entity.HasOne(d => d.UserInfoRecieve).WithMany(p => p.CompanyDebitsDetailsUserRecieved)
                .HasForeignKey(d => d.UserRecievedId)
                .HasConstraintName("FK_CompanyDebitDetails_UserInfoRecieve");

            //entity.HasOne(e => e.UserInfo)
            //      .WithMany()
            //      .HasForeignKey(e => e.UserId)
            //      // REMOVE this line: .HasPrincipalKey(p => p.Id)
            //      .OnDelete(DeleteBehavior.Restrict)
            //      .HasConstraintName("FK_CompanyDebitDetails_UserInfo");

            //entity.HasOne(e => e.UserInfoRecieve)
            //      .WithMany()
            //      .HasForeignKey(e => e.UserRecievedId)
            //      // REMOVE this line: .HasPrincipalKey(p => p.Id)
            //      .OnDelete(DeleteBehavior.Restrict)
            //      .HasConstraintName("FK_CompanyDebitDetails_UserInfoRecieve");

            // Indexes for better performance
            entity.HasIndex(e => e.CompDebitId);
            entity.HasIndex(e => e.CompDebitDate);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.UserRecievedId);
        });

        modelBuilder.Entity<ViolationInfo>(entity =>
        {
            entity.ToTable("ViolationInfo");

            
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);
            entity.Property(e => e.TransfereToDebit).HasDefaultValue(0);
            entity.Property(e => e.ViolationCost).HasColumnType("money");
            entity.Property(e => e.ViolationNo).HasMaxLength(50);
          
            entity.Property(e => e.ViolationPlace).HasMaxLength(500);
            entity.Property(e => e.ViolationTime).HasColumnType("datetime");
          

            entity.HasOne(d => d.Employee).WithMany(p => p.ViolationInfos)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_ViolationInfo_EmployeeInfo");
            
            entity.HasOne(d => d.Car).WithMany(p => p.ViolationInfos)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK_ViolationInfo_CarInfo");

            entity.HasOne(d => d.User).WithMany(p => p.ViolationInfos)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ViolationInfo_PasswordData");

            entity.HasOne(d => d.ViolationGuide).WithMany(p => p.ViolationInfos)
                .HasForeignKey(d => d.ViolationGuideId)
                .HasConstraintName("FK_ViolationInfo_Deff");

        });
        modelBuilder.Entity<EmployeeTakeMoney>(entity =>
        {
            entity.Property(e => e.TakeMoney)
                .HasColumnType("decimal(18,2)"); // Add this line

            // Configure TakeUser relationship
            entity.HasOne(d => d.TakeUser)
                  .WithMany(p => p.EmployeeTakeMoneyTakeUser)
                  .HasForeignKey(d => d.TakeUserId)
                  .HasConstraintName("FK_EmployeeTakeMoneyTakeUser_PasswordData");

            // Configure User relationship
            entity.HasOne(d => d.User)
                  .WithMany(p => p.EmployeeTakeMoneyUser)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("FK_EmployeeTakeMoneyUser_PasswordData");
        });
        modelBuilder.Entity<DeffEmpTreatment>(entity =>
        {
            entity.ToTable("DeffEmpTreatment");

            entity.Property(e => e.TreatmentAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.DeleteFlag).HasDefaultValue(0);

            entity.HasOne(d => d.Employee).WithMany(p => p.DeffEmpTreatments)
                .HasForeignKey(d => d.EmpId)
                .HasConstraintName("FK_DeffEmpTreatment_EmployeeInfo");

            entity.HasOne(d => d.DeffTreatment).WithMany(p => p.DeffEmpTreatments)
                .HasForeignKey(d => d.DeffId)
                .HasConstraintName("FK_DeffEmpTreatment_Deff");

            entity.HasOne(d => d.User).WithMany(p => p.DeffEmpTreatments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_DeffEmpTreatment_PasswordData");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
public DbSet<DeffInformation> DeffInformation { get; set; } = default!;

}
