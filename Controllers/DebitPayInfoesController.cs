using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;

namespace RahalWeb.Controllers
{
    public class DebitPayInfoesController : Controller
    {
        private readonly RahalWebContext _context;

        public DebitPayInfoesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: DebitPayInfoes
        public async Task<IActionResult> Index(int? EmpCodeString, string? EmpSearch, int? DefTypeId, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
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
            var query = _context.DebitPayInfos
                 .FromSqlRaw($"SELECT * FROM DebitPayInfo where DebitInfoId IN(Select Id from DebitInfo where EmpId IN (Select ID From EmployeeInfo where CompanyId IN({companyIdsString})))")
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c=>c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag==0)
                .OrderBy(e => e.DebitPayNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            //if (FromDateSearch == null && ToDateSearch == null)
            //{
            //    var today = DateOnly.FromDateTime(DateTime.Now);
            //    var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
            //    query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= sevenDaysAgo);
            //}
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
            // Pagination

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitPayQty);

            ViewBag.TotalBillPayed = totalBillPayed;

            int pageSize = 50; // Set your page size
            return View(await PaginatedList<DebitPayInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));


            //var rahalWebContext = _context.DebitPayInfos.Include(d => d.DebitInfo).Include(d => d.User).Include(d => d.UserRecieved).Include(d => d.Violation);
            //return View(await rahalWebContext.ToListAsync());
        }

        // GET: DebitPayInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos
                .Include(d => d.DebitInfo)
                .Include(d => d.User)
                .Include(d => d.UserRecieved)
                .Include(d => d.ViolationInfo )
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }

            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Create
        public IActionResult Create()
        {
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id");
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id");
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id");
            return View();
        }

        // POST: DebitPayInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DebitPayNo,DebitPayDate,DebitPayQty,DeleteFlag,ViolationId,UserId,UserRecievedId,UserRecievedDate,Hent,DeleteReson,DebitInfoId")] DebitPayInfo debitPayInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(debitPayInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos.FindAsync(id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // POST: DebitPayInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DebitPayNo,DebitPayDate,DebitPayQty,DeleteFlag,ViolationId,UserId,UserRecievedId,UserRecievedDate,Hent,DeleteReson,DebitInfoId")] DebitPayInfo debitPayInfo)
        {
            if (id != debitPayInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(debitPayInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DebitPayInfoExists(debitPayInfo.Id))
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
            ViewData["DebitInfoId"] = new SelectList(_context.DebitInfos, "Id", "Id", debitPayInfo.DebitInfoId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", debitPayInfo.UserRecievedId);
            ViewData["ViolationId"] = new SelectList(_context.Deffs, "Id", "Id", debitPayInfo.ViolationId);
            return View(debitPayInfo);
        }

        // GET: DebitPayInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var debitPayInfo = await _context.DebitPayInfos
                .Include(d => d.DebitInfo)
                .Include(d => d.User)
                .Include(d => d.UserRecieved)
                .Include(d => d.ViolationInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (debitPayInfo == null)
            {
                return NotFound();
            }

            return View(debitPayInfo);
        }

        // POST: DebitPayInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var debitPayInfo = await _context.DebitPayInfos.FindAsync(id);
            if (debitPayInfo != null)
            {
                _context.DebitPayInfos.Remove(debitPayInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DebitPayInfoExists(int id)
        {
            return _context.DebitPayInfos.Any(e => e.Id == id);
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
            var query = _context.DebitPayInfos
                .FromSqlRaw($"select * from DebitPayInfo where DebitInfoId in (Select Id from DebitInfo where EmpId in ( Select Id from  EmployeeInfo where deleteFlag = 0 and  CompanyId  IN ({companyIdsString})))")
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag==0)
                .OrderBy(e => e.DebitPayNo);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.FullNameAr!.Contains(EmpSearch));
            }

            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.DebitTypeId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitInfo!.Emp!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<DebitPayInfo>)query.Where(e => e.DebitPayDate >= sevenDaysAgo);
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
            // Pagination

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.DebitPayQty);

            ViewBag.TotalBillPayed = totalBillPayed;

            int pageSize = 10; // Set your page size
            return View(await PaginatedList<DebitPayInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));


            //var rahalWebContext = _context.DebitPayInfos.Include(d => d.DebitInfo).Include(d => d.User).Include(d => d.UserRecieved).Include(d => d.Violation);
            //return View(await rahalWebContext.ToListAsync());
        }

    }
}
