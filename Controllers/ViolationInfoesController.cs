using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;



namespace RahalWeb.Controllers
{
    public class ViolationInfoesController : Controller
    {
        private readonly RahalWebContext _context;

        public ViolationInfoesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: ViolationInfoes
        public async Task<IActionResult> Index(int? CarCodeSearch, int? EmpCodeSearch, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData!.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);


            var query = _context.ViolationInfos
             .FromSqlRaw($"SELECT * FROM ViolationInfo where EmpID IN (Select Id from EmployeeInfo where CompanyId IN({companyIdsString}))")
             .Include(v => v.Car)
             .Include(v => v.Employee)
             .Include(v => v.User)
             .Include(v => v.ViolationGuide)
             .AsQueryable(); // Start as IQueryable

            if (FromDateSearch.HasValue)
            {
                query = query.Where(e => e.ViolationDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));
            }
            if (ToDateSearch.HasValue)
            {
                query = query.Where(e => e.ViolationDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }


            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = query.Where(e => e.ViolationDate >= sevenDaysAgo);
            }


            if (CarCodeSearch.HasValue)
            {
                query = query.Where(e => e.Car!.CarCode == CarCodeSearch);
            }
            if (EmpCodeSearch.HasValue)
            {
                query = query.Where(e => e.Employee!.EmpCode == EmpCodeSearch);
            }

            // Store current search values for the view
            ViewData["CarCodeFilter"] = CarCodeSearch;
            ViewData["EmpNoFilter"] = EmpCodeSearch;

           

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

        

            // Pagination
            int pageSize = 50; // Set your page size
            return View(await PaginatedList<ViolationInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
       }

        // GET: ViolationInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationInfo = await _context.ViolationInfos
                .Include(v => v.Car)
                .Include(v => v.Employee)
                .Include(v => v.User)
                .Include(v => v.ViolationGuide)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (violationInfo == null)
            {
                return NotFound();
            }

            return View(violationInfo);
        }

        // GET: ViolationInfoes/Create
        public IActionResult Create()
        {
            ViewBag.CarId = new SelectList(_context.CarInfos
                       .Where(b => _context.Contracts
                        .Where(c => c.DeleteFlag == 0 && c.Status == 0)
                        .Any(c => c.CarId == b.Id))
                       ,"Id","CarNo");

            
           
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr");
            ViewBag.UserId = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewBag.ViolationGuideId = new SelectList(_context.Deffs
                                            .Where (a=>a.DeffType==26 ), "Id", "DeffName");
            return View();
        
        }

        // POST: ViolationInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( ViolationInfo violationInfo)
        {
            if (ModelState.IsValid)
            {
                var empId = _context.Contracts
                             .Where(a => a.Car!.Id == violationInfo.CarId && a.DeleteFlag==0 && a.Status==0)
                             .Select(a => a.EmployeeId)
                             .SingleOrDefault();
                violationInfo.EmpId = empId;
                _context.Add(violationInfo);
                await _context.SaveChangesAsync();

                // add debit for violation
                //Int32 maxDebitNo = _context.DebitInfos
                //                .Where(a => a.DebitNo != null)
                //                .Max(a => Convert.ToInt32(a.DebitNo));

                int maxDebitId = _context.DebitInfos.Max(a => Convert.ToInt32(a.Id));
                var maxDebitNo = 1;
                var newDebitInfos = new DebitInfo
                {
                    DebitNo = maxDebitNo+1,
                    EmpId = empId,
                    DebitTypeId = 0,
                    DebitDate = DateOnly.FromDateTime(DateTime.Now),
                    DebitQty = violationInfo.ViolationCost,
                    DeleteFlag = 0,
                    DebitDescrp = "مخالفة",
                    UserId = violationInfo.UserId,
                    ViolationId = maxDebitId ,
                    DebitPayed = 0,
                    DebitRemaining = violationInfo.ViolationCost
                };
                _context.Add(newDebitInfos);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CarId = new SelectList(_context.CarInfos, "Id", "CarNo");
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos, "Id", "FullNameAr");
            ViewBag.UserId = new SelectList(_context.PasswordData, "Id", "UserFullName");
            ViewBag.ViolationGuideId = new SelectList(_context.Deffs
                                            .Where(a => a.DeffType == 26), "Id", "DeffName");
            return View(violationInfo);
        }

        // GET: ViolationInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationInfo = await _context.ViolationInfos.FindAsync(id);
            if (violationInfo == null)
            {
                return NotFound();
            }
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "Id", violationInfo.CarId);
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", violationInfo.EmpId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", violationInfo.UserId);
            ViewData["ViolationGuideId"] = new SelectList(_context.Deffs, "Id", "Id", violationInfo.ViolationGuideId);
            return View(violationInfo);
        }

        // POST: ViolationInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CarId,ViolationDate,ViolationTime,ViolationPlace,ViolationSpeed,ViolationGuideId,ViolationCost,ViolationPoint,TransfereToDebit,DeleteFlag,ViolationBookNo,EmpId,ViolationNo,UserId")] ViolationInfo violationInfo)
        {
            if (id != violationInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(violationInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViolationInfoExists(violationInfo.Id))
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
            ViewData["CarId"] = new SelectList(_context.CarInfos, "Id", "Id", violationInfo.CarId);
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", violationInfo.EmpId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", violationInfo.UserId);
            ViewData["ViolationGuideId"] = new SelectList(_context.Deffs, "Id", "Id", violationInfo.ViolationGuideId);
            return View(violationInfo);
        }

        // GET: ViolationInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var violationInfo = await _context.ViolationInfos
                .Include(v => v.Car)
                .Include(v => v.Employee)
                .Include(v => v.User)
                .Include(v => v.ViolationGuide)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (violationInfo == null)
            {
                return NotFound();
            }

            return View(violationInfo);
        }

        // POST: ViolationInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var violationInfo = await _context.ViolationInfos.FindAsync(id);
            if (violationInfo != null)
            {
                _context.ViolationInfos.Remove(violationInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViolationInfoExists(int id)
        {
            return _context.ViolationInfos.Any(e => e.Id == id);
        }
    }
}
