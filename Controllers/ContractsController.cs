using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RahalWeb.Data;
using RahalWeb.Extensions;
using RahalWeb.Models;
using RahalWeb.Models.MyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class ContractsController : Controller
    {
        private readonly RahalWebContext _context;

        public ContractsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, int? ContractType)
        {

            var timePeriods = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "يومي" },
            new SelectListItem { Value = "1", Text = "شهري" }
        };
            ViewBag.TimePeriods = new SelectList(timePeriods, "Value", "Text");

            // Get companies for dropdown
            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                    .Where(c => c.DeleteFlag == 0)
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id",
                "CompNameAr",
                companyId);



            // Base query with includes
            var query = _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0 && m.Status == 0)
                .OrderBy(e => e.ContractNo);



            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Car!.CarCode == CarCodeString);
            }
            if (ContractType.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.ContractType == ContractType);
            }


            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            // Store current search values for the view
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpNameFilter"] = EmpNameSearch;
            ViewData["CompanyFilter"] = companyId;

            // Pagination
            int pageSize = 10; // Set your page size
            return View(await PaginatedList<Contract>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));


        }


        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        [HttpGet]
        public JsonResult CompanyEmp(int Id)
        {

            var branches = _context.EmployeeInfos
              .Where(b => b.CompanyId == Id && b.DeleteFlag == 0 &&
                     !_context.Contracts
                         .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                         .Any(c => c.EmployeeId == b.Id)) // ← Use the correct column name here
              .Select(b => new {
                  id = b.Id,
                  fullNameAr = b.FullNameAr
              })
              .ToList();

            return Json(branches);
        }
    
        public JsonResult CompanyCar(int Id)
        {

            var branches = _context.CarInfos
                        .Where(b => b.CompanyId == Id &&
                     !_context.Contracts
                         .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                         .Any(c => c.CarId == b.Id)) // ← Use the correct column name here
                        .Select(b => new {
                            id = b.Id,
                            carNo = b.CarNo
                        })
                        .ToList();

            return Json(branches);
        }



        #region "Daily Contract"

        public async Task<IActionResult> IndexDaily(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, string? ContractNoString)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            //var timePeriods = new List<SelectListItem>
            //{
            //    new SelectListItem { Value = "0", Text = "يومي" },
            //    new SelectListItem { Value = "1", Text = "شهري" }
            //};
            //ViewBag.TimePeriods = new SelectList(timePeriods, "Value", "Text");

            // Get companies for dropdown
            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                    .Where(c => c.DeleteFlag == 0)
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id",
                "CompNameAr",
                companyId);



            // Base query with includes
            var query = _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0 && m.Status == 0 && m.ContractType == 0)
                .OrderBy(e => e.ContractNo);


            if (!string.IsNullOrEmpty(ContractNoString))
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.ContractNo!.Contains(ContractNoString));
            }


            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Car!.CarCode == CarCodeString);
            }
            //if (ContractType.HasValue)
            //{
            //    query = (IOrderedQueryable<Contract>)query.Where(e => e.ContractType == ContractType);
            //}


            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            // Store current search values for the view
            ViewData["ContractNoFilter"] = ContractNoString;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpNameFilter"] = EmpNameSearch;
            ViewData["CompanyFilter"] = companyId;

            // Contracts with DiscountDate within the next 7 days
            var today = DateOnly.FromDateTime(DateTime.Today);
            var in7Days = today.AddDays(7);
            var approachingDiscount = await _context.Contracts
                .Include(c => c.Employee)
                .Include(c => c.Car)
                .Where(m => m.DeleteFlag == 0 && m.Status == 0 && m.ContractType == 0
                    && m.DiscountDate.HasValue
                    && m.DiscountDate.Value >= today
                    && m.DiscountDate.Value <= in7Days)
                .ToListAsync();
            ViewBag.ApproachingDiscountContracts = approachingDiscount;

            // Pagination
            int pageSize = 10; // Set your page size
            return View(await PaginatedList<Contract>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult CreateDaily()

        {

            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;

            
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo");
            ViewData["EmployeeId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            // ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos, "Id", "CompNameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDaily(Contract contract, int maxContractNo)
        {
            if (ModelState.IsValid)
            {
                maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));
                contract.ContractNo = Convert.ToString(maxContractNo + 1);
                contract.UserId = 1;
                contract.ContractType = 0;
                contract.DeleteFlag = 0;
                contract.Status = 0;
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDaily));
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo", contract.CarId);
            //ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", contract.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", contract.UserId);
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos, "Id", "CompNameAr");
            return View(contract);
        }


        [HttpGet]
        public async Task<IActionResult> DetailsDaily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }



        #endregion

        // POST: Contracts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.



        #region "ContractMonthly"
        public async Task<IActionResult> IndexMonthly(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, string? ContractNoString)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            if (companyIds.Any())
            {
                ViewBag.Companies = new SelectList(
                            await _context.CompanyInfos
                                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                                .OrderBy(c => c.CompNameAr)
                                .ToListAsync(),
                            "Id",
                            "CompNameAr",
                            companyId);
            }
            else
            {
                ViewBag.Companies = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            // Base query with includes
            var query = _context.Contracts
                .FromSqlRaw($"Select * from Contract where deleteFlag = 0 and status = 0 and  EmployeeId in (Select Id from EmployeeInfo where DeleteFlag = 0 and CompanyId  IN ({companyIdsString}))")
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0 && m.Status == 0 && m.ContractType == 1)
                .OrderBy(e => e.ContractNo);


            if (!string.IsNullOrEmpty(ContractNoString))
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.ContractNo!.Contains(ContractNoString));
            }


            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Car!.CarCode == CarCodeString);
            }
            //if (ContractType.HasValue)
            //{
            //    query = (IOrderedQueryable<Contract>)query.Where(e => e.ContractType == ContractType);
            //}


            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Contract>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            // Store current search values for the view
            ViewData["ContractNoFilter"] = ContractNoString;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpNameFilter"] = EmpNameSearch;
            ViewData["CompanyFilter"] = companyId;

            // Pagination
            //int pageSize = 50; // Set your page size
            return View(query);

//            return View(await PaginatedList<Contract>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult CreateMonthly()

        {

            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

           

            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo");
            ViewData["EmployeeId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            // ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                                    .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                                    .Where(c=>c.DeleteFlag == 0), "Id", "CompNameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMonthly(Contract contract, int maxContractNo)
        {

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            // check if the credit start date > start date make it error and not save the contract 
            if (contract.CreditStartDate < contract.StartDate)
            {
                ModelState.AddModelError("DebitQty", "بداية القسط أكبر من بداية العقد");
                return View();
            }

            if (ModelState.IsValid)
            {
                maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));
                contract.ContractNo = Convert.ToString(maxContractNo + 1);
                contract.UserId = HttpContext.Session.GetInt32("UserId"); 
                contract.ContractType = 1;
                contract.DeleteFlag = 0;
                contract.Status = 0;
                contract.Employee = null;
                contract.Car = null;
                _context.Add(contract);
                await _context.SaveChangesAsync();

                int? contractId = _context.Contracts
                              .Where(a => a.ContractNo == Convert.ToString(maxContractNo + 1))
                              .Select(a => a.Id)
                              .FirstOrDefault();
                if (contract.HaveVacation == true)    // when select have vacation 
                {
                    DateOnly tempDate = new DateOnly(contract!.StartDate!.Value.Year,
                                                    contract.StartDate.Value.Month, 1)
                                                   .AddMonths(1)
                                                   .AddDays(-1);
                    DateOnly tempCreditDate = new DateOnly(1900, 1, 1);
                    if (contract.CreditTotalCost != 0)
                    {
                        tempCreditDate = (DateOnly)contract!.CreditStartDate!;
                    }
                    int countMonth = 1;
                    while (tempDate < contract.EndDate)
                    {
                        if (countMonth != 13)
                        {
                            if (tempDate.Month == tempCreditDate.Month && tempDate.Year == tempCreditDate.Year && tempCreditDate <= contract.CreditEndDate)
                            {
                                var newContractDetails = new ContractDetail
                                {
                                    ContractId = contractId,
                                    DailyCreditDate = tempDate,
                                    DailyCredit = contract.DailyCredit,

                                    Status = 0,
                                    CarCredit = contract.CreditMonthPay,
                                    DeleteFlag = 0
                                };
                                _context.ContractDetails.Add(newContractDetails);
                                await _context.SaveChangesAsync(); // Use SaveChanges() if not async

                                tempDate  = DateOnly.FromDateTime(new DateTime(tempDate.Year, tempDate.Month, 1)
                                                                    .AddMonths(2)
                                                                    .AddDays(-1));
                                tempCreditDate = tempCreditDate.AddMonths(1);
                            }
                            else
                            {
                                var newContractDetails = new ContractDetail
                                {
                                    ContractId = contractId,
                                    DailyCreditDate = tempDate,
                                    DailyCredit = contract.DailyCredit,
                                    Status = 0,
                                    CarCredit = 0,
                                    DeleteFlag = 0
                                };
                                _context.ContractDetails.Add(newContractDetails);
                                await _context.SaveChangesAsync(); // Use SaveChanges() if not async 
                                tempDate = DateOnly.FromDateTime(new DateTime(tempDate.Year, tempDate.Month, 1)
                                                                    .AddMonths(2)
                                                                    .AddDays(-1));
                            }
                        }
                        else
                        {
                            if (tempCreditDate <= contract.CreditEndDate)
                            {
                                var newContractDetails = new ContractDetail
                                {
                                    ContractId = contractId,
                                    DailyCreditDate = tempDate,
                                    DailyCredit = 0,

                                    Status = 2,
                                    CarCredit = contract.CreditMonthPay,
                                    DeleteFlag = 0
                                };
                                _context.ContractDetails.Add(newContractDetails);
                                await _context.SaveChangesAsync(); // Use SaveChanges() if not async
                                tempCreditDate = tempCreditDate.AddMonths(1);
                            }
                            else
                            {
                                var newContractDetails = new ContractDetail
                                {
                                    ContractId = contractId,
                                    DailyCreditDate = tempDate,
                                    DailyCredit = 0,

                                    Status = 2,
                                    CarCredit = 0,
                                    DeleteFlag = 0
                                };
                                _context.ContractDetails.Add(newContractDetails);
                                await _context.SaveChangesAsync(); // Use SaveChanges() if not async
                            }
                            var newVacation = new Vacation
                            {
                                EmpId = contract.EmployeeId,                          // Employee ID (nullable int)
                                FromDate = tempDate, // Start date (nullable DateOnly)
                                ToDate = tempDate.AddMonths(1).AddDays(-1),  // End date (nullable DateOnly)
                                NoOfDays = (tempDate.AddMonths(1).DayNumber - tempDate.DayNumber),                       // Number of vacation days (nullable int)
                                VacationPayed = 0,                  // 1 for paid, 0 for unpaid (nullable int)
                                DeleteFlag = 0,                     // 0 for active, 1 for deleted (nullable int)
                                VacationStatus = 0                  // Status (e.g., 1 for approved, 0 for pending) (nullable int)
                            };
                            _context.Vacations.Add(newVacation);
                            await _context.SaveChangesAsync(); // Use SaveChanges() if not async 
                            tempDate =  DateOnly.FromDateTime(new DateTime(tempDate.Year, tempDate.Month, 1)
                                                    .AddMonths(2)
                                                    .AddDays(-1));
                            countMonth = 0;
                        }
                        countMonth++;
                    }
                }
                else   // without vacation
                {
                    DateOnly tempDate = new DateOnly(contract!.StartDate!.Value.Year,
                                                    contract.StartDate.Value.Month,1)
                                                   .AddMonths(1)
                                                   .AddDays(-1);
                    DateOnly tempCreditDate = new DateOnly(1900, 1, 1);
                    if (contract.CreditTotalCost != 0)
                    {
                        tempCreditDate = (DateOnly)contract!.CreditStartDate!;
                    }

                    int countMonth = 1;
                    while (tempDate < contract.EndDate)
                    {
                        if (tempDate.Month == tempCreditDate.Month && tempCreditDate <= contract.CreditEndDate)
                        {
                            var newContractDetails = new ContractDetail
                            {
                                ContractId = contractId,
                                DailyCreditDate = tempDate,
                                DailyCredit = contract.DailyCredit,

                                Status = 0,
                                CarCredit = contract.CreditMonthPay,
                                DeleteFlag = 0
                            };
                            _context.ContractDetails.Add(newContractDetails);
                            await _context.SaveChangesAsync(); // Use SaveChanges() if not async

                            tempDate = DateOnly.FromDateTime(new DateTime(tempDate.Year, tempDate.Month, 1)
                                                                    .AddMonths(2)
                                                                    .AddDays(-1));

                            tempCreditDate = tempCreditDate.AddMonths(1);
                        }
                        else
                        {
                            var newContractDetails = new ContractDetail
                            {
                                ContractId = contractId,
                                DailyCreditDate = tempDate,
                                DailyCredit = contract.DailyCredit,

                                Status = 0,
                                CarCredit = 0,
                                DeleteFlag = 0
                            };
                            _context.ContractDetails.Add(newContractDetails);
                            await _context.SaveChangesAsync(); // Use SaveChanges() if not async 
                            tempDate =  DateOnly.FromDateTime(new DateTime(tempDate.Year, tempDate.Month, 1)
                                                                    .AddMonths(2)
                                                                    .AddDays(-1));
                        }
                        countMonth++;
                    }
                }
                // should insert vacation here
                return RedirectToAction(nameof(IndexMonthly));
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo", contract.CarId);
            //ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", contract.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", contract.UserId);
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");
            return View(contract);
        }


        [HttpGet]
        public async Task<IActionResult> DetailsMonthly(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            ViewBag.CreditPayed = await _context.ContractDetails
                                .Where(d => d.Status == 3 && d.ContractId == id)
                                .SumAsync(d => d.CarCredit);
            ViewBag.CreditNotPayed = await _context.ContractDetails
                              .Where(d => d.Status == 0 && d.ContractId == id)
                              .SumAsync(d => d.CarCredit);

            if (contract == null)
            {
                return NotFound();
            }   

            return View(contract);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeMonthly(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            var lastBillNumber = _context.Bills
                .Where(b => b.ContractId == contract.Id && b.DeleteFlag == 0)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            if (lastBillNumber != null)
            {

                ViewBag.LastBill = lastBillNumber;


            }

            ViewBag.TtalPay = _context.Bills.Where(b => b.EmployeeId == contract.EmployeeId &&
                                              b.DeleteFlag == 0)
                                        .Sum(b => b.BillPayed);


            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;



            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo");


            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");


            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMonthly(int id, Contract contract)
        {

            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexMonthly));
            }

          
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EndContractMonthly(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewBag.totalPay = _context.Bills.Where(b => b.EmployeeId == contract.EmployeeId &&
                                             b.DeleteFlag == 0 && b.ContractId == contract.Id)
                                       .Sum(b => b.BillPayed);
            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> EndContractMonthlyAsync(Contract contract)
        {

            if (contract != null)
            {
                var contractToUpdate = new Contract
                {
                    Id = contract.Id,
                    Status = 1,
                    ContractEndDate = contract.ContractEndDate,
                    ContractEndReson = contract.ContractEndReson
                };

                _context.Attach(contractToUpdate);

                // Mark all the fields you want to update as modified
                var entry = _context.Entry(contractToUpdate);
                entry.Property(x => x.Status).IsModified = true;
                entry.Property(x => x.ContractEndDate).IsModified = true;
                entry.Property(x => x.ContractEndReson).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexMonthly));

        }

        [HttpGet]
        public async Task<IActionResult> IndexArchive(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, string? ContractNoString)
        {
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
             
            ViewBag.Companies = new SelectList(
              await _context.CompanyInfos
                  .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                  .Where(c => c.DeleteFlag == 0)
                  .OrderBy(c => c.CompNameAr)
                  .ToListAsync(),
              "Id",
              "CompNameAr",
              companyId);


            var query = _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Where(m => m.Status == 1)
                .OrderByDescending(e => e.ContractNo)
                .Select(c => new ContractCreditData
                {
                    Contract = c,
                    TotalPaid = _context.Bills
                        .Where(b => b.ContractId == c.Id)
                        .Sum(b => (decimal?)b.BillPayed) ?? 0
                });

            if (!string.IsNullOrEmpty(ContractNoString))
            {
                query = (IOrderedQueryable<ContractCreditData>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoString));
            }

            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = (IOrderedQueryable<ContractCreditData>)query.Where(e => e.Contract!.Car!.CarCode == CarCodeString);
            }
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<ContractCreditData>)query.Where(e => e.Contract!.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<ContractCreditData>)query.Where(e => e.Contract!.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<ContractCreditData>)query.Where(e => e.Contract!.Employee!.CompanyId == companyId.Value);
            }

            int pageSize = 10; // Set your page size
            return View(await PaginatedList<ContractCreditData>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult GetEmployeeByCode(int empCode)
        {
            var employee = _context.EmployeeInfos
                   .Where(a => a.DeleteFlag == 0 && a.EmpCode == empCode)
                   .Where(a => !_context.Contracts
                       .Any(c => c.EmployeeId == a.Id && c.DeleteFlag == 0 && c.Status == 0))
                   .Include(e => e.Company)
                   .FirstOrDefault();

            if (employee == null)
            {
                return NotFound();
            }

            return Json(new
            {
                companyId = employee.CompanyId,
                employeeId = employee.Id
            });
        }
        public IActionResult GetCarByCode(int carCode)
        {
            var car = _context.CarInfos
                      .Include(c => c.Company)
                      .FirstOrDefault(c => c.CarCode == carCode &&
                                          c.DeleteFlag == 0 &&
                                          c.Company!.DeleteFlag == 0 &&  // Add company delete flag check
                                          !_context.Contracts.Any(contract => contract.CarId == c.Id &&
                                                                            contract.DeleteFlag == 0 &&
                                                                            contract.Status == 0));


            if (car == null)
            {
                return NotFound();
            }

            decimal PayedCredit = (decimal)(_context.ContractDetails
                                  .Where(cd => cd.Contract!.CarId == car!.Id && cd.Status == 3)
                                  .Sum(cd => (decimal?)cd.CarCredit) ?? 0);

            
            if(car.NoOfCredit != 0)
            {
                decimal RemainingCredit = (car.NoOfCredit * car.CarCredit) - PayedCredit;
 
                car.NoOfCredit = (int)(RemainingCredit / car.CarCredit);
                car.CarCredit = RemainingCredit;
               
            }
            else
            {
                car.CarCredit = 0;
                car.NoOfCredit = 0;
            }


                return Json(new
                {
                    companyId = car.CompanyId,
                    carId = car.Id,
                    carCredit = car.CarCredit, // Add this line
                    noOfCredit = car.NoOfCredit  // Add this line
                });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeMonthlyRent(int id)
        {


            if (id == null)
            {
                return NotFound();
            }
            
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            var lastBillNumber = _context.Bills
                .Where(b => b.ContractId == contract.Id && b.DeleteFlag == 0)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            if (lastBillNumber != null)
            {

                ViewBag.LastBill = lastBillNumber;


            }

            ViewBag.TtalPay = _context.Bills.Where(b => b.EmployeeId == contract.EmployeeId &&
                                              b.DeleteFlag == 0)
                                        .Sum(b => b.BillPayed);

            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                  .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                  , "Id", "CompNameAr");
            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMonthlyRent(int id, Contract contract,DateOnly StartDateRent)
        {

            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    _context.Update(contract);


                    var contractDetailsToUpdate = _context.ContractDetails
                             .Where(cd => cd.ContractId == contract.Id && cd.DailyCreditDate > StartDateRent  && cd.Status==0)
                             .ToList();

                    foreach (var detail in contractDetailsToUpdate)
                    {
                        detail.DailyCredit = contract.DailyCredit;
                    }

                    _context.SaveChanges();

                    await _context.SaveChangesAsync();



                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexMonthly));
            }


            return View();
        }
        #endregion

        #region "ContractDaily"
        [HttpGet]
        public async Task<IActionResult> EndContractDaily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewBag.totalPay = _context.Bills.Where(b => b.EmployeeId == contract.EmployeeId &&
                                             b.DeleteFlag == 0 && b.ContractId == contract.Id)
                                       .Sum(b => b.BillPayed);
            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> EndContractDailyAsync(Contract contract)
        {

            if (contract != null)
            {
                var contractToUpdate = new Contract
                {
                    Id = contract.Id,
                    Status = 1,
                    ContractEndDate = contract.ContractEndDate,
                    ContractEndReson = contract.ContractEndReson
                };

                _context.Attach(contractToUpdate);

                // Mark all the fields you want to update as modified
                var entry = _context.Entry(contractToUpdate);
                entry.Property(x => x.Status).IsModified = true;
                entry.Property(x => x.ContractEndDate).IsModified = true;
                entry.Property(x => x.ContractEndReson).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexDaily));

        }
        #endregion
        #region "Credit"
        public async Task<IActionResult> IndexCredit(int? CarCodeString, int? EmpCodeString, string? EmpNameSearch, int? companyId, int? pageNumber, string? ContractNoString)
        {
            // Get companies for dropdown
            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                    .Where(c => c.DeleteFlag == 0)
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id",
                "CompNameAr",
                companyId);

            // Base query with includes
            var query = _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0 && m.Status == 0 && m.ContractType == 1 &&
                           (m.CreditTotalCost != 0 && m.CreditTotalCost != 0))
                .OrderBy(e => e.ContractNo)
                .Select(c => new ContractCreditData
                {
                    Contract = c,
                    TotalPaid = _context.CreditBills
                        .Where(b => b.Contract!.CarId == c.CarId)
                        .Sum(b => (decimal?)b.CreditBillPayed) ?? 0
                });

            if (!string.IsNullOrEmpty(ContractNoString))
            {
                query = query.Where(e => e.Contract.ContractNo!.Contains(ContractNoString));
            }

            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = query.Where(e => e.Contract.Car!.CarCode == CarCodeString);
            }

            if (EmpCodeString.HasValue)
            {
                query = query.Where(e => e.Contract.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = query.Where(e => e.Contract.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (companyId.HasValue)
            {
                query = query.Where(e => e.Contract.Employee!.CompanyId == companyId.Value);
            }

            //decimal totalBillPayed = (decimal)query.Sum(item => item.TotalPaid);

            //ViewBag.TotalBillPayed = totalBillPayed;

            // Store current search values for the view
            ViewData["ContractNoFilter"] = ContractNoString;
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpNameFilter"] = EmpNameSearch;
            ViewData["CompanyFilter"] = companyId;

            // Pagination
            // int pageSize = 20; // Set your page size
            //            return View(await PaginatedList<ContractCreditData>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
            var result = await query.ToListAsync();
            return View(result);
        }
        public IActionResult DetailsCredit()
        {
            return View();
        }
        #endregion

        public IActionResult Create()

        {
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;

            var timePeriods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "يومي" },
                    new SelectListItem { Value = "1", Text = "شهري" }
                };
            ViewBag.TimePeriods = new SelectList(timePeriods, "Value", "Text");

            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "CarNo");
            ViewData["EmployeeId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            // ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "Id", contract.CarId);
            ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", contract.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", contract.UserId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "Id", contract.CarId);
            ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", contract.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", contract.UserId);
            return View(contract);
        
        }

        // POST: Contracts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "Id", contract.CarId);
            ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", contract.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", contract.UserId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }


        public IActionResult PayDetails(int? id)
        {

            return View();
        }
        [HttpGet]
       public async Task<IActionResult> AddCar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            var contract = await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            var lastBillNumber = _context.Bills
                .Where(b => b.ContractId == contract.Id && b.DeleteFlag == 0)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            if (lastBillNumber != null)
            {

                ViewBag.LastBill = lastBillNumber;


            }

            ViewBag.TtalPay = _context.Bills.Where(b => b.EmployeeId == contract.EmployeeId &&
                                              b.DeleteFlag == 0)
                                        .Sum(b => b.BillPayed);

            int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

            ViewBag.MaxContractNo = maxContractNo + 1;

            var availableCars = _context.CarInfos
                                .Where(car =>car.DeleteFlag == 0 &&
                                !_context.Contracts.Any(contract => contract.CarId == car.Id  && contract.DeleteFlag == 0 && contract.Status==0 ))
                                .ToList();

            ViewData["CarId"] = new SelectList(availableCars, "Id", "CarNo");



            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                  .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                  , "Id", "CompNameAr");
            return View(contract);

        }
        [HttpPost]
        public async Task<IActionResult> AddCar(int id, Contract contract , DateOnly StartDateRent,DateOnly StartDateCarCredit , string CarCreditDisplay  ,  int NoOfCreditDisplay)
        {


            decimal carCredit = 0m;
            if (!string.IsNullOrWhiteSpace(CarCreditDisplay))
            {
                // Clean the string first
                string cleanValue = CarCreditDisplay
                    .Replace(",", "")
                    .Replace("$", "")
                    .Replace("€", "")
                    .Replace("£", "")
                    .Replace(" ", "")
                    .Trim();

                if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedValue))
                {
                    carCredit = parsedValue;
                }
                else
                {
                    // Log the problematic value
                    //_logger.LogWarning($"Could not parse CarCreditDisplay: '{CarCreditDisplay}'");
                }
            }

            if (StartDateRent > StartDateCarCredit)
            {
                ModelState.AddModelError(string.Empty, "تاريخ بداية استقطاع القسط لا يمكن ان يكون قبل تاريخ بداية ايجار السيارة.");
                return View(contract);
            }
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int maxContractNo = _context.Contracts.Max(a => Convert.ToInt32(a.ContractNo));

                    ViewBag.MaxContractNo = maxContractNo + 1;

                    // Create a new contract with the same data
                    var newContract = new Contract
                    {
                        // Copy all properties from the original contract
                        ContractNo = ViewBag.MaxContractNo?.ToString(),
                        DailyCredit = contract.DailyCredit,
                        Status = 0,
                        EmployeeId = contract.EmployeeId,
                        CarId = contract.CarId,
                        ContractDate = contract.ContractDate,
                        StartDate = contract.StartDate,
                        EndDate= contract.EndDate,
                        NoOfDays = contract.NoOfDays,
                        DeleteFlag = 0,
                        TotalCost = contract.TotalCost,
                        ContractEndDate = contract.ContractEndDate ,
                        ContractEndReson = contract.ContractEndReson,
                        ContractType= contract.ContractType,
                        UserId = HttpContext.Session.GetInt32("UserId") ,
                        CreditStartDate = contract.CreditStartDate,
                        CreditEndDate = contract.CreditEndDate,
                        CreditNoOfMonth = NoOfCreditDisplay,
                        CreditMonthPay = NoOfCreditDisplay == 0 ? 0 : carCredit / NoOfCreditDisplay,
                        CreditTotalCost = carCredit ,  // contract.CreditTotalCost
                        HaveVacation = contract.HaveVacation,
                    };

                    // Add the new contract to context
                    _context.Contracts.Add(newContract);

                    int maxContractNoNew = _context.Contracts.Max(a => Convert.ToInt32(a.Id));
                    maxContractNoNew = maxContractNoNew + 1;
                    contract.Status = 1;
                    _context.Update(contract);

                    var contractDetailsToUpdate = _context.ContractDetails
                             .Where(cd => cd.ContractId == contract.Id  && cd.Status != 3 && cd.DailyCreditDate >= StartDateRent)
                             .ToList();

                    int TempNoOfCredit = NoOfCreditDisplay; 

                    foreach (var detail in contractDetailsToUpdate)
                    {
                        detail.ContractId = maxContractNoNew;
                        if ( detail.DailyCreditDate >= StartDateCarCredit && TempNoOfCredit >0)
                        {
                            detail.CarCredit = carCredit / NoOfCreditDisplay;
                            TempNoOfCredit--;
                        }
                        else
                        {
                            detail.CarCredit = 0;
                        }

                        //  detail.DailyCredit = contract.DailyCredit;
                    }

                    _context.SaveChanges();

                    await _context.SaveChangesAsync();



                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexMonthly));
            }


            return View();
        }


    }
}
