using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Data;

namespace RahalWeb.Controllers
{
    public class BillsController : Controller
    {
        private readonly RahalWebContext _context;

        public object UserId { get; private set; }

        public BillsController(RahalWebContext context)
        {
            _context = context;
        }
        // GET: Bills
        public async Task<IActionResult> Index(string sortField,string sortOrder, string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch, int? UserId)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            //// Get the user's company data from TempData
            //// Get the user's company data from TempData
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Select(int.Parse).ToList();
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

            var query = _context.Bills
                .FromSqlRaw($"Select * from Bill where EmployeeId In ( Select Id From EmployeeInfo where CompanyId  IN ({companyIdsString}))")
                .Include(c => c.Employee)
                .Include(c => c.Contract)
                .Include(c => c.Contract!.Car).Where(a=>a.DeleteFlag == 0 )
                .OrderByDescending(c => c.Id);
 
           //.Select(car => new
           //{
           //    Car = car,
           //    PaidCredits = _context.ContractDetails
           //    .Where(cd => cd.Contract!.CarId == car.Id && cd.Status == 3)
           //    .Sum(cd => (decimal?)cd.CarCredit) ?? 0,
           //    RemainingCredits = _context.ContractDetails
           //    .Where(cd => cd.Contract!.CarId == car.Id && cd.Status == 0)
           //    .Sum(cd => (decimal?)cd.CarCredit) ?? 0,
           //});


            ViewBag.UserId = new SelectList(
                         await _context.PasswordData
                            .OrderBy(c => c.UserFullName)
                            .ToListAsync(),
                        "Id",
                        "UserFullName",
                        UserId);

            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.Car!.CarNo!.Contains(CarNoSearch));
            }

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (!string.IsNullOrEmpty(ContractNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (UserId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.UserId == UserId.Value);
            }
            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
            ViewData["ContractNoFilter"] = ContractNoSearch;
            ViewData["CompanyFilter"] = companyId;
            ViewData["UserFilter"] = UserId;

            if (FromDateSearch == null)
            {
                ViewData["FromDateFilter"] = "";
            }
            else
            {
                ViewData["FromDateFilter"] = FromDateSearch.Value.ToString("yyyy-MM-dd");
            }
            if (ToDateSearch == null)
            {
                ViewData["ToDateFilter"] = "";
            }
            else
            {
                ViewData["ToDateFilter"] = ToDateSearch.Value.ToString("yyyy-MM-dd");
            }
            // Convert to string

            decimal totalBillPayed = query.Sum(item => item.BillPayed)!.Value;

            ViewBag.TotalBillPayed = totalBillPayed;



            @ViewData["EmpNameFilter"] = EmpNameSearch;

            switch (sortField)
            {
                case "BillNo":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.BillNo) : query.OrderBy(b => b.BillNo);
                    break;
                case "ContractNo":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.Contract!.ContractNo) : query.OrderBy(b => b.Contract!.ContractNo);
                    break;
                case "CarNo":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.Contract!.Car!.CarNo) : query.OrderBy(b => b.Contract!.Car!.CarNo);
                    break;
                case "EmpCode":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.Employee!.EmpCode) : query.OrderBy(b => b.Employee!.EmpCode);
                    break;
                case "FullNameAr":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.Employee!.FullNameAr) : query.OrderBy(b => b.Employee!.FullNameAr);
                    break;
                case "BillDate":
                    query = sortOrder == "desc" ? query.OrderByDescending(b => b.BillDate) : query.OrderBy(b => b.BillDate);
                    break;
                // Add cases for other sortable fields...
                default:
                    query = query.OrderByDescending(b => b.BillDate);
                    break;
            }
            // Pagination
            int pageSize = 100; // Set your page size
            return View(await PaginatedList<Bill>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #region "Daily"

        public async Task<IActionResult> IndexDaily(string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch , string? EmpNameSearch)
        {

            ViewBag.Companies = new SelectList(
               await _context.CompanyInfos
                   .Where(c => c.DeleteFlag == 0)
                   .OrderBy(c => c.CompNameAr)
                   .ToListAsync(),
               "Id",
               "CompNameAr",
               companyId);


           
            var query = _context.Bills
                .Include(c => c.Employee)
                .Include(c=>c.Contract)
                .Include(c=>c.User)
                .Where(a => a.Contract!.ContractType == 0);

            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.Car!.CarNo!.Contains(CarNoSearch));
            }

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }


            if (!string.IsNullOrEmpty(ContractNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }

            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= sevenDaysAgo);
            }

            decimal totalBillPayed = query.Sum(item => item.BillPayed)!.Value;

            ViewBag.TotalBillPayed = totalBillPayed;


            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
            ViewData["ContractNoFilter"] = ContractNoSearch;
            ViewData["CompanyFilter"] = companyId;

            if (FromDateSearch == null)
            {
                ViewData["FromDateFilter"] = "";
            }
            else
            {
                ViewData["FromDateFilter"] = FromDateSearch.Value.ToString("yyyy-MM-dd");
            }
            if (ToDateSearch == null)
            {
                ViewData["ToDateFilter"] = "";
            }
            else
            {
                ViewData["ToDateFilter"] = ToDateSearch.Value.ToString("yyyy-MM-dd");
            }
                // Convert to string
               
            @ViewData["EmpNameFilter"] = EmpNameSearch;

            // Pagination
            int pageSize = 10; // Set your page size
            return View(await PaginatedList<Bill>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

 
        public IActionResult GetLastBillForEmployee(int employeeId)
        {
            // Query your database to get the last bill number for this employee
            var lastBillNumber = _context.Bills
                .Where(b => b.EmployeeId == employeeId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            var billCost = _context.Contracts.Where(a => a.EmployeeId == employeeId && a.DeleteFlag == 0 && a.Status == 0).ToList();

            if (billCost != null)
            {
                ViewBag.billCost = billCost[0];

            }
            else
            {
                ViewBag.billCost = "";
            }

                return Json(lastBillNumber);
        }
        public IActionResult GetLastBillForEmployeeMonthly(int employeeId)
        {
            // Query your database to get the last bill number for this employee
            var lastBillNumber = _context.Bills
                .Where(b => b.EmployeeId == employeeId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            var billCost = _context.Contracts
                .Where(a => a.EmployeeId == employeeId && a.DeleteFlag == 0 && a.Status == 0)
                .FirstOrDefault(); // Use FirstOrDefault instead of ToList since we only need the first record

            if (lastBillNumber != null)
            {
                // If we have a bill, return it along with the contract data
                return Json(new
                {
                    billNo = lastBillNumber.BillNo,
                    fromDate = lastBillNumber.FromDate,
                    toDate = lastBillNumber.ToDate,
                    billDate = lastBillNumber.BillDate,
                    billPayed = lastBillNumber.BillPayed,
                    noOfDays = lastBillNumber.NoOfDays,
                    dailyCredit = billCost?.DailyCredit,
                    startDate = billCost?.StartDate
                });
            }
            else if (billCost != null)
            {
                // If no bill but we have contract data, return just the contract data
                return Json(new
                {
                    dailyCredit = billCost.DailyCredit,
                    startDate = billCost.StartDate
                });
            }

            // If neither bill nor contract data exists
            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDailyAsync(int? id)
        {
            int maxBillNo = _context.Bills.Max(a => a.BillNo)!.Value;
            ViewBag.maxBillNo = maxBillNo + 1;

            var contract = await _context.Contracts.Include(c=>c.Employee).Where(a=>a.Id == id).FirstOrDefaultAsync();
            
            
            ViewBag.EmployeeId = contract?.Employee?.FullNameAr ?? "Not available";
            ViewBag.EmpId = contract!.EmployeeId;
            
            ViewBag.dailyCost = contract.DailyCredit;

            var lastBillNumber = _context.Bills
                            .Where(b => b.EmployeeId == contract!.EmployeeId)
                            .OrderByDescending(b => b.Id)
                            .FirstOrDefault();

            ViewBag.LastBill = lastBillNumber; 
            if (lastBillNumber == null)
            {
                ViewBag.fromDate = contract.StartDate;
                
            }
            else
            {
                DateOnly toDate = lastBillNumber.ToDate.GetValueOrDefault(DateOnly.FromDateTime(DateTime.Now));
                ViewBag.fromDate = toDate.AddDays(1);
            }

                var bank = _context.Deffs.Where(a => a.DeffType == 9).ToList();

            ViewBag.BankId = new SelectList(bank, "Id", "DeffName");

            //ViewBag.EmployeeId = new SelectList(employeesWithContracts, "Id", "FullNameAr"); 


            return View();

        }

        [HttpPost]
        public async Task<IActionResult> CreateDaily( Bill bill)
        {
            if (ModelState.IsValid)
            {
                bill.Id = 0;
                if (bill.BankBillNo == null)
                {
                    bill.BankIntNo = 568;
                }

                DateOnly currentDate = DateOnly.FromDateTime( DateTime.Now);
                int daysDifference =  currentDate.DayNumber - bill.ToDate!.Value.DayNumber;

                bill.LateDays = daysDifference;


                bill.BillTime= TimeOnly .FromDateTime(DateTime.Now);
                var contractid = _context.Contracts.Where(a => a.EmployeeId == bill.EmployeeId && a.DeleteFlag == 0 && a.Status == 0)
                      .Select(a => a.Id)
                      .FirstOrDefault(); // or .SingleOrDefault() if you expect exactly one

                bill.ContractId = contractid;
                
                bill.BillHent = "إيجار يومي";

                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexDaily));
            }

            return View(bill);
        }

        public async Task<IActionResult> DetailsDaily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);

        }


        [HttpGet]
        public async Task<IActionResult> DeleteDaily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDaily(Bill bill)
        {
            var BillToUpdate = new Bill
            {
                Id = bill.Id,
                DeleteFlag = 1,

            };

            _context.Attach(BillToUpdate);
            // Mark all the fields you want to update as modified
            var entry = _context.Entry(BillToUpdate);
            entry.Property(x => x.DeleteFlag).IsModified = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexMonthly));
        }


        #endregion
        #region "Monthly"
        public async Task<IActionResult> IndexMonthly(string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch)
        {

            ViewBag.Companies = new SelectList(
               await _context.CompanyInfos
                   .Where(c => c.DeleteFlag == 0)
                   .OrderBy(c => c.CompNameAr)
                   .ToListAsync(),
               "Id",
               "CompNameAr",
               companyId);

            var query = _context.Bills
                .Include(c => c.Employee)
                .Include(c => c.Contract)
                .Include(c => c.User)
                .Where(a => a.Contract!.ContractType == 1);

            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.Car!.CarNo!.Contains(CarNoSearch));
            }

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }


            if (!string.IsNullOrEmpty(ContractNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= sevenDaysAgo);
            }

            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
            ViewData["ContractNoFilter"] = ContractNoSearch;
            ViewData["CompanyFilter"] = companyId;

            if (FromDateSearch == null)
            {
                ViewData["FromDateFilter"] = "";
            }
            else
            {
                ViewData["FromDateFilter"] = FromDateSearch.Value.ToString("yyyy-MM-dd");
            }
            if (ToDateSearch == null)
            {
                ViewData["ToDateFilter"] = "";
            }
            else
            {
                ViewData["ToDateFilter"] = ToDateSearch.Value.ToString("yyyy-MM-dd");
            }
            // Convert to string

            decimal? totalBillPayed = query.Sum(item => item.BillPayed);

            ViewBag.TotalBillPayed = totalBillPayed;



            @ViewData["EmpNameFilter"] = EmpNameSearch;

            // Pagination
            int pageSize = 10; // Set your page size
            return View(await PaginatedList<Bill>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));

        }


        [HttpGet]
        public IActionResult CreatMonthly()
        {
            int maxBillNo = _context.Bills.Max(a => a.BillNo)!.Value;

            ViewBag.maxBillNo = maxBillNo + 1;

            var employeesWithContracts = _context.EmployeeInfos.Where(employee =>
                                         _context.Contracts.Any(contract => contract.EmployeeId == employee.Id && contract.DeleteFlag == 0 && contract.Status == 0 && contract.ContractType==1))
                                        .OrderBy(a => a.FullNameAr)
                                        .ToList();

            var bank = _context.Deffs.Where(a => a.DeffType == 9).ToList();

            ViewBag.BankId = new SelectList(bank, "Id", "DeffName");
            ViewBag.EmployeeId = new SelectList(employeesWithContracts, "Id", "FullNameAr");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatMonthly(Bill bill)
        {
            if (ModelState.IsValid)
            {

                if (bill.BankBillNo == null)
                {
                    bill.BankIntNo = 568;
                }

                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
                int daysDifference = currentDate.Month  - bill.ToDate!.Value.Month;
                if (daysDifference >0 )
                {
                    bill.LateDays = daysDifference;
                }
                else
                {
                    bill.LateDays = 0;
                }
   
                bill.BillTime = TimeOnly.FromDateTime(DateTime.Now);
                var contractid = _context.Contracts.Where(a => a.EmployeeId == bill.EmployeeId && a.DeleteFlag == 0 && a.Status == 0)
                      .Select(a => a.Id)
                      .FirstOrDefault(); // or .SingleOrDefault() if you expect exactly one
                bill.ContractId = contractid;
                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexMonthly));
            }

            return View(bill);
        }


        public async Task<IActionResult> DetailsMonthly(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);

        }

        [HttpGet]
        public async Task<IActionResult> DeleteMonthly(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills.Include(e=>e.Employee).Include(c=>c.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            var contraactDetails = _context.ContractDetails.Where(e => e.ContractId == bill.ContractId && e.Status == 3 
                                                                    && e.DailyCreditDate > bill.ToDate);
            if (!contraactDetails.Any())
            {
                ViewBag.CanDelete = true;
            }
            else
            {
                ViewBag.CanDelete = false;
            }


            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMonthly(Bill bill)
        {
            var BillToUpdate = new Bill
            {
                Id = bill.Id,
                DeleteFlag = 1,
               
            };

            _context.Attach(BillToUpdate);
            // Mark all the fields you want to update as modified
            var entry = _context.Entry(BillToUpdate);
            entry.Property(x => x.DeleteFlag).IsModified = true;

            await _context.SaveChangesAsync();

            var contractDetails = _context.ContractDetails
            .Where(c => c.ContractId == bill.ContractId && c.BillId == bill.Id  && c.Status !=2);

            if (contractDetails.Any())
            {
                await contractDetails.ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.Status, 0));
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        #endregion
        // GET: Bills/Details/5
        // GET: Bills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.Id == id);
        }
        public IActionResult ExportToExcel(
     string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch, int? UserId)
        {
            try
            {
                // Get filtered data (reuse your existing filtering logic)
                var bills = GetFilteredBills( CarNoSearch, EmpNoSearch, ContractNoSearch, companyId, pageNumber, FromDateSearch, ToDateSearch, EmpNameSearch, UserId);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("فواتير التحصيل");

                    // Set RTL direction for the worksheet
                    worksheet.RightToLeft = true;

                    // Add headers
                    var headers = new string[]
                    {
                "رقم الفاتورة", "رقم العقد", "رقم السيارة", "رقم السائق",
                "الرقم المدني", "اسم السائق", "تاريخ الفاتورة", "الوقت",
                "من تاريخ", "إلى تاريخ", "عدد الأيام", "المبلغ",
                "نوع العقد", "ملاحظات"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // Add data
                    int row = 2;
                    foreach (var bill in bills)
                    {
                        worksheet.Cell(row, 1).Value = bill.BillNo;
                        worksheet.Cell(row, 2).Value = bill.Contract?.ContractNo;
                        worksheet.Cell(row, 3).Value = bill.Contract?.Car?.CarNo;
                        worksheet.Cell(row, 4).Value = bill.Employee?.EmpCode;
                        worksheet.Cell(row, 5).Value = bill.Employee?.CivilId;
                        worksheet.Cell(row, 6).Value = bill.Employee?.FullNameAr;
                        worksheet.Cell(row, 7).Value = bill.BillDate?.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 8).Value = bill.BillTime?.ToString();
                        worksheet.Cell(row, 9).Value = bill.FromDate?.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 10).Value = bill.ToDate?.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 11).Value = bill.NoOfDays;
                        worksheet.Cell(row, 12).Value = bill.BillPayed;
                        worksheet.Cell(row, 13).Value = bill.Contract?.ContractType == 0 ? "يومي" : "شهري";
                        worksheet.Cell(row, 14).Value = bill.BillHent;

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Format currency column
                    worksheet.Column(12).Style.NumberFormat.Format = "#,##0.00";

                    // Create memory stream
                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset stream position

                    string fileName = $"فواتير_التحصيل_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    // This will trigger the browser's "Save As" dialog
                    return File(
                        fileStream: stream,
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: fileName);
                }
            }
            catch (Exception ex)
            {
                // Log error
                return RedirectToAction("Index");
            }
        }

        // Helper method to get filtered data (reuse your existing filtering logic)
        private IQueryable<Bill> GetFilteredBills(
           string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch, int? UserId)
        {
            var query = _context.Bills
             .Include(c => c.Employee)
             .Include(c => c.Contract)
             .Include(c => c.Contract!.Car).Where(a => a.DeleteFlag == 0).OrderByDescending(c => c.Id);


            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.Car!.CarNo!.Contains(CarNoSearch));
            }

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }

            if (!string.IsNullOrEmpty(ContractNoSearch))
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (UserId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(e => e.UserId == UserId.Value);
            }

            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= sevenDaysAgo);
            }
            return query;
        }
    }
}
