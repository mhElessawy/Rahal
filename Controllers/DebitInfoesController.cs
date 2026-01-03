using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class DebitInfoesController : Controller
    {
        private readonly RahalWebContext _context;

        public DebitInfoesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: DebitInfoes
        public async Task<IActionResult> Index(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, int? pageNumber , DateTime? FromDateSearch, DateTime? ToDateSearch)
        {

            // Get companies for dropdown
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            //// Get the user's company data from TempData
            //// Get the user's company data from TempData
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


            // Get Car Type for dropdown
            ViewBag.DefTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 20)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);



            // Base query with includes
            var query = _context.DebitInfos
                 .FromSqlRaw($"Select * from DebitInfo where EmpId in( Select Id from  EmployeeInfo where CompanyId IN ({companyIdsString}))")
                .Include(c => c.Emp)
                .Include(c => c.DebitType)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0)
                .OrderBy(e => e.DebitNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.FullNameAr! .Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate >= sevenDaysAgo);
            }
            // Store current search values for the view
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["DefTypeFilter"] = DefTypeId;
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



            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitQty);

            ViewBag.TotalBillPayed = totalBillPayed;



            // Pagination
            int pageSize = 50; // Set your page size
            return View(await PaginatedList<DebitInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // GET: DebitInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitInfo = await _context.DebitInfos.Include(c=>c.Emp).Include(c=>c.DebitType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitInfo == null)
            {
                return NotFound();
            }

            return View(debitInfo);
        }

        // GET: DebitInfoes/Create
        public async Task<IActionResult> Create()
        {
            int? maxDebitNo = (int?)_context.DebitInfos.Max(a => a.DebitNo);

            ViewBag.MaxDebitNo = maxDebitNo + 1;


            ViewBag.EmployeeId = new SelectList( 
                                    _context.EmployeeInfos
                                       .Where(a => a.DeleteFlag == 0),

                                    "Id",
                                    "FullNameAr");

            ViewBag.DefTypeId = new SelectList(
                await _context.Deffs
                   .Where(c => c.DeffType == 20)
                   .OrderBy(c => c.DeffName)
                   .ToListAsync(),
               "Id",
               "DeffName");

            return View();
        }

        // POST: DebitInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DebitInfo debitInfo)
        {
            if (ModelState.IsValid)
            {
                debitInfo.UserId = HttpContext.Session.GetInt32("UserId") ;
                debitInfo.ViolationId = null;
                debitInfo.DebitPayed = 0;
                debitInfo.DebitRemaining = debitInfo.DebitQty;
                debitInfo.DeleteFlag = 0;
                debitInfo.Emp = null;

                _context.Add(debitInfo);
                await _context.SaveChangesAsync();
                
                int DebitId = _context.DebitInfos.Max(a => Convert.ToInt32(a.Id));

                return RedirectToAction("PayPrint", "DebitInfoes", new { Id = DebitId });

                //return RedirectToAction(nameof(Index));
            }
            return View(debitInfo);
        }

        public IActionResult PayPrint(int? Id)
        {

            if (Id == null)
            {
                return NotFound();
            }
            var printBill = _context.DebitInfos.Include(c => c.Emp).Include(c => c.User).Include(c=>c.DebitType).Where(c => c.Id == Id).FirstOrDefault();

            if (printBill == null)
            {
                return NotFound();
            }

            return View(printBill);
        }

        // GET: DebitInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
           

            var debitInfo = await _context.DebitInfos.FindAsync(id);
            if (debitInfo == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeId = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr",debitInfo.EmpId);
            ViewBag.DefTypeId = new SelectList(
                await _context.Deffs
                   .Where(c => c.DeffType == 20)
                   .OrderBy(c => c.DeffName)
                   .ToListAsync(),
               "Id",
               "DeffName",debitInfo.DebitTypeId);


            return View(debitInfo);
        }

        // POST: DebitInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DebitInfo debitInfo)
        {
            if (id != debitInfo.Id)
            {
                return NotFound();
            }

            // Custom validation checks
            if (debitInfo.DebitQty <= 0)
            {
                ModelState.AddModelError("DebitQty", "المبلغ يجب أن يكون أكبر من الصفر");
            }

            if (debitInfo.DebitPayed > debitInfo.DebitQty)
            {
                ModelState.AddModelError("DebitPayed", "المدفوع لا يمكن أن يكون أكبر من المبلغ");
            }

            // Only proceed if ModelState is valid
            if (ModelState.IsValid)
            {
                try
                {
                    debitInfo.UserId = HttpContext.Session.GetInt32("UserId"); 
                    debitInfo.ViolationId = 569;
                    debitInfo.DebitRemaining = debitInfo.DebitQty - debitInfo.DebitPayed;
                    debitInfo.DeleteFlag = 0;

                    _context.Update(debitInfo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebitInfoExists(debitInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If validation fails, return to the edit view with error messages
            return View(debitInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {

            int? maxDebitPayNo = (int?)_context.DebitPayInfos.Max(a => a.DebitPayNo);

            ViewBag.MaxDebitPayNo = maxDebitPayNo + 1;


            var debitInfo = await _context.DebitInfos.FindAsync(id);
            if (debitInfo == null)
            {
                return NotFound();
            }

            var employee = _context.EmployeeInfos.FirstOrDefault(e => e.Id == debitInfo.EmpId);

            if (employee != null)
            {
               
                ViewBag.EmployeeFullNameAr = employee.FullNameAr;
            }
            ViewBag.DefTypeId = new SelectList(
                await _context.Deffs
                   .Where(c => c.DeffType == 20)
                   .OrderBy(c => c.DeffName)
                   .ToListAsync(),
               "Id",
               "DeffName", debitInfo.DebitTypeId);

            return View(debitInfo);
        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, DebitInfo debitInfo, decimal DebitPayQty, int DebitPayNo)
        {
            // Custom validation checks
            if (DebitPayQty <= 0)
            {
                ModelState.AddModelError("DebitQty", "المبلغ يجب أن يكون أكبر من الصفر");
            }

            if (DebitPayQty > debitInfo.DebitRemaining)
            {
                ModelState.AddModelError("DebitPayed", "المدفوع لا يمكن أن يكون أكبر من المبلغ");
            }

            // Only proceed if ModelState is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing DebitInfo from database
                    var existingDebitInfo = await _context.DebitInfos
                        .Include(d => d.Emp)       // Include related Employee if needed
                        .Include(d => d.DebitType) // Include related DebitType if needed
                        .FirstOrDefaultAsync(d => d.Id == debitInfo.Id);

                    if (existingDebitInfo == null)
                    {
                        return NotFound();
                    }

                    // Update only the necessary fields
                    existingDebitInfo.UserId = HttpContext.Session.GetInt32("UserId"); 

                    existingDebitInfo.ViolationId = debitInfo.ViolationId ;
                    existingDebitInfo.DebitRemaining = existingDebitInfo.DebitQty - (existingDebitInfo.DebitPayed + DebitPayQty);
                    existingDebitInfo.DebitPayed += DebitPayQty;
                    existingDebitInfo.DeleteFlag = 0;

                    // No need to call Update if you fetched the entity with tracking
                    // _context.Update(existingDebitInfo);

                    var debitPayInfo = new DebitPayInfo
                    {
                        DebitPayNo = DebitPayNo,
                        DebitPayDate = DateOnly.FromDateTime(DateTime.Now),
                        DebitPayQty = DebitPayQty,
                        DeleteFlag = 0,
                        ViolationId = existingDebitInfo.ViolationId,
                        UserId = existingDebitInfo.UserId,
                        UserRecievedId = existingDebitInfo.UserId,
                        DebitInfoId = existingDebitInfo.Id,
                    };

                    _context.DebitPayInfos.Add(debitPayInfo);

                    await _context.SaveChangesAsync();

                    int DebitPayId = _context.DebitPayInfos.Max(a => Convert.ToInt32(a.Id));

                    return RedirectToAction("DebitPayPrint", "DebitInfoes", new { Id = DebitPayId });
                  //  return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebitInfoExists(debitInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            int? maxDebitPayNo = (int?)_context.DebitPayInfos.Max(a => a.DebitPayNo);

            ViewBag.MaxDebitPayNo = maxDebitPayNo + 1;

            var employee = _context.EmployeeInfos.FirstOrDefault(e => e.Id == debitInfo.EmpId);

            if (employee != null)
            {

                ViewBag.EmployeeFullNameAr = employee.FullNameAr;
            }
            ViewBag.DefTypeId = new SelectList(
                await _context.Deffs
                   .Where(c => c.DeffType == 20)
                   .OrderBy(c => c.DeffName)
                   .ToListAsync(),
               "Id",
               "DeffName", debitInfo.DebitTypeId);
            // If validation fails, return to the edit view with error messages
            return View(debitInfo);
        }


        public IActionResult DebitPayPrint(int Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var printBill = _context.DebitPayInfos.Include(c => c.DebitInfo).Include(c => c.User).Where(c => c.Id == Id).FirstOrDefault();

            if (printBill == null)
            {
                return NotFound();
            }
            return View(printBill);
        }


        // GET: DebitInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitInfo = await _context.DebitInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitInfo == null)
            {
                return NotFound();
            }

            return View(debitInfo);
        }

        // POST: DebitInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var debitInfo = await _context.DebitInfos.FindAsync(id);
            if (debitInfo != null)
            {
                _context.DebitInfos.Remove(debitInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool DebitInfoExists(int id)
        {
            return _context.DebitInfos.Any(e => e.Id == id);
        }


        public async Task<IActionResult> IndexReport(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {

            // Get companies for dropdown
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            //// Get the user's company data from TempData
            //// Get the user's company data from TempData
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


            // Get Car Type for dropdown
            ViewBag.DefTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 20)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);



            // Base query with includes
            var query = _context.DebitInfos
                .FromSqlRaw($"Select * from DebitInfo where EmpId in( Select Id from  EmployeeInfo where CompanyId IN ({companyIdsString}))")
                .Include(c => c.Emp)
                .Include(c => c.DebitType)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0)
                .OrderBy(e => e.DebitNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.FullNameAr!.Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<DebitInfo>)query.Where(e => e.DebitDate >= sevenDaysAgo);
            }
            // Store current search values for the view
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
            ViewData["DefTypeFilter"] = DefTypeId;
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

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitQty);
       
            ViewBag.TotalBillPayed = totalBillPayed;

            // Pagination
            int pageSize = 50; // Set your page size
            return View(await PaginatedList<DebitInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult GetEmployeeByCode(int empCode)
        {
            var employee = _context.EmployeeInfos
                .Include(e => e.Company)
                .FirstOrDefault(e => e.EmpCode == empCode && e.DeleteFlag == 0 );

            if (employee == null)
            {
                return NotFound();
            }

            return Json(new
            {
               employeeId = employee.Id
            });
        }
    }
}
