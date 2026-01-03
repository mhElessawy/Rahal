using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class PurshasesController : Controller
    {
        private readonly RahalWebContext _context;

        public PurshasesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: Purshases
        public async Task<IActionResult> Index(string? CarNoSearch, int? EmpNoSearch, int? companyId, int? DefTypeId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch)
        {

            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            TempData["PurshaseShowAll"] = HttpContext.Session.GetString("PurshaseShowAll");

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

            // Get Car Type for dropdown
            ViewBag.DefTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 17)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);

            var query = _context.Purshases.Include(c=>c.PurshaseNavigation)
                .Where(a => a.DeleteFlag == 0);


            if (TempData["PurshaseShowAll"]?.ToString().Equals("False", StringComparison.OrdinalIgnoreCase) == true)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.UserId == (int)ViewData["UserId"]!);
            }
   


                if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.CompId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate >= sevenDaysAgo);
            }

            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
           
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

            decimal totalBillPayed = query.Sum(item => item.PurshasePayed)!.Value;

            ViewBag.TotalBillPayed = totalBillPayed;

            ViewData["DefTypeFilter"] = DefTypeId;

            ViewData["EmpNameFilter"] = EmpNameSearch;

            // Pagination
            int pageSize = 50; // Set your page size
            return View(await PaginatedList<Purshase>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
         
        }

        // GET: Purshases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purshase = await _context.Purshases
               
                .Include(p => p.PurshaseNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purshase == null)
            {
                return NotFound();
            }

            return View(purshase);
        }

        // GET: Purshases/Create
        public async Task<IActionResult> Create()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            int maxBillNo = _context.Purshases.Max(a => a.PurshaseNo)!.Value;
            ViewBag.maxPurshaseNo = maxBillNo + 1;


            ViewData["CarId"] = new SelectList(_context.CarInfos
                .FromSqlRaw($"Select * from CarInfo where CompanyId in ({companyIdsString})")
                .Where(a=>a.DeleteFlag ==0 ), "Id", "CarNo");
            ViewData["CompId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "CompNameAr");
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos
                .FromSqlRaw($"Select * from EmployeeInfo where CompanyId in ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "FullNameAr");
            ViewData["PurshaseId"] = new SelectList(await _context.Deffs.Where(c => c.DeffType == 17 && c.DeleteFlag==0)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName");

            return View();
        }

        // POST: Purshases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Purshase purshase)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            if (ModelState.IsValid)
            {

                purshase.DeleteFlag = 0;
                purshase.UserId = HttpContext.Session.GetInt32("UserId");
                if (purshase.CarId == 0 || !_context.CarInfos.Any(c => c.Id == purshase.CarId))
                {
                    purshase.CarId = null;
                }

                _context.Add(purshase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos
               .FromSqlRaw($"Select * from CarInfo where CompanyId in ({companyIdsString})")
               .Where(a => a.DeleteFlag == 0), "Id", "CarNo");
            ViewData["CompId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "CompNameAr");
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos
                .FromSqlRaw($"Select * from EmployeeInfo where CompanyId in ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "FullNameAr");
            ViewData["PurshaseId"] = new SelectList(await _context.Deffs.Where(c => c.DeffType == 17 && c.DeleteFlag == 0)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName");

            return View(purshase);
        }

        // GET: Purshases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            var purshase = await _context.Purshases.FindAsync(id);
            if (purshase == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos
                .FromSqlRaw($"Select * from CarInfo where CompanyId in ({companyIdsString})")
                , "Id", "CarNo", purshase.CarId);
            ViewData["CompId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "Id", purshase.CompId);
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos
                .FromSqlRaw($"Select * from EmployeeInfo where CompanyId in ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "FullNameAr", purshase.EmpId);
            ViewData["PurshaseId"] = new SelectList(await _context.Deffs.Where(c => c.DeffType == 17 && c.DeleteFlag == 0)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName", purshase.PurshaseId);
            return View(purshase);
        }

        // POST: Purshases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Purshase purshase)
        {
            if (id != purshase.Id)
            {
                return NotFound();
            }
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purshase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurshaseExists(purshase.Id))
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
            ViewData["CarId"] = new SelectList(_context.CarInfos
               .FromSqlRaw($"Select * from CarInfo where CompanyId in ({companyIdsString})")
               , "Id", "CarNo", purshase.CarId);
            ViewData["CompId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "Id", purshase.CompId);
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos
                .FromSqlRaw($"Select * from EmployeeInfo where CompanyId in ({companyIdsString})")
                .Where(a => a.DeleteFlag == 0), "Id", "FullNameAr", purshase.EmpId);
            ViewData["PurshaseId"] = new SelectList(_context.Deffs, "Id", "Id", purshase.PurshaseId);
            return View(purshase);
        }

        // GET: Purshases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purshase = await _context.Purshases
               .Include(p => p.PurshaseNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purshase == null)
            {
                return NotFound();
            }

            return View(purshase);
        }

        // POST: Purshases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purshase = await _context.Purshases.FindAsync(id);
            if (purshase != null)
            {
                try
                {
                    purshase.DeleteFlag = 1; 
                    _context.Update(purshase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurshaseExists(purshase.Id))
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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurshaseExists(int id)
        {
            return _context.Purshases.Any(e => e.Id == id);
        }
        public async Task<IActionResult> IndexReport(string? CarNoSearch, int? EmpNoSearch, int? companyId, int? DefTypeId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch, int? UserId)
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
                    .Where(c => c.DeffType == 17)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                DefTypeId);

            ViewBag.userId = new SelectList(
                         await _context.PasswordData
                            .OrderBy(c => c.UserFullName)
                            .ToListAsync(),
                        "Id",
                        "UserFullName",
                        UserId);



            var query = _context.Purshases.Include(c => c.PurshaseNavigation)
                .Where(a => a.DeleteFlag == 0);





            if (DefTypeId.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseId == DefTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.CompId == companyId.Value);
            }


            if (UserId.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.UserId == UserId.Value);
            }



            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<Purshase>)query.Where(e => e.PurshaseDate >= sevenDaysAgo);
            }

            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
            
            ViewData["UserFilter"] = UserId;
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

            decimal totalBillPayed = query.Sum(item => item.PurshasePayed)!.Value;

            ViewBag.TotalBillPayed = totalBillPayed;

            ViewData["DefTypeFilter"] = DefTypeId;

            ViewData["EmpNameFilter"] = EmpNameSearch;

            // Pagination
            int pageSize = 30; // Set your page size
            return View(await PaginatedList<Purshase>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

    }
}
