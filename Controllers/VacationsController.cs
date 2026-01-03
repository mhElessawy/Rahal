using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class VacationsController : Controller
    {
        private readonly RahalWebContext _context;

        public VacationsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: Vacations
        public async Task<IActionResult> Index(int? EmpNoSearch, string? EmpNameSearch, int? pageNumber)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            // Base query with includes
            var query = _context.Vacations
                .FromSqlRaw($"Select * from Vacation where EmpId in(Select Id from EmployeeInfo where CompanyId in ({companyIdsString}))")
                .Include(c => c.Emp)
                .Where(a=>a.DeleteFlag==0)
                .OrderBy(e => e.FromDate);

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<Vacation>)query.Where(e => e.Emp!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<Vacation>)query.Where(e => e.Emp!.FullNameAr!.Contains(EmpNameSearch));
            }

            // Store current search values for the view
           
            ViewData["EmpCodeFilter"] = EmpNoSearch;
            ViewData["EmpNameFilter"] = EmpNameSearch;
           
            // Pagination
            int pageSize = 50; // Set your page size
            return View(await PaginatedList<Vacation>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

        // GET: Vacations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations.Include(c=>c.Emp)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacation == null)
            {
                return NotFound();
            }

            return View(vacation);
        }

        // GET: Vacations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vacations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vacation vacation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vacation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vacation);
        }

        // GET: Vacations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation == null)
            {
                return NotFound();
            }

            ViewBag.WorkMonth = new SelectList(
                _context.ContractDetails
                    .Where(a => a.Contract!.EmployeeId == vacation.EmpId && a.Status == 0)
                    .Select(a => new {
                        Id = a.Id,
                        DateText = a.DailyCreditDate // Use directly if already formatted
                    }),
                "Id",
                "DateText");

            var employee = _context.EmployeeInfos.FirstOrDefault(e => e.Id == vacation.EmpId);

            if (employee != null)
            {

                ViewBag.EmployeeFullNameAr = employee.FullNameAr;
                ViewBag.EmpCode = employee.EmpCode;
            }

            return View(vacation);
        }

        // POST: Vacations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vacation vacation, string ContractDetailsId, string? OldFromDate, string? OldToDate)
        {
            if (id != vacation.Id)
            {
                return NotFound();
            }
            // change contractDetails Vacation
            if (int.Parse(ContractDetailsId) != 0)
            {
                ContractDetail? NewVacation = (ContractDetail?)_context.ContractDetails.Where(a => a.Id == int.Parse(ContractDetailsId)).FirstOrDefault();

                if (NewVacation != null)
                {
                    // استخدام التاريخ القديم للمقارنة
                    DateOnly oldToDateValue = DateOnly.MinValue;
                    if (!string.IsNullOrEmpty(OldToDate))
                    {
                        oldToDateValue = DateOnly.Parse(OldToDate);
                    }

                    ContractDetail? changeVacation = (ContractDetail?)_context.ContractDetails
                        .Where(a => a.Contract!.Employee!.Id == vacation.EmpId && a.DailyCreditDate == oldToDateValue)
                        .FirstOrDefault();

                    if (changeVacation != null)
                    {
                        changeVacation.DailyCredit = NewVacation.DailyCredit;
                        changeVacation.Status = 0;
                        _context.Update(changeVacation);
                        await _context.SaveChangesAsync();

                        NewVacation.Status = 2;
                        NewVacation.DailyCredit = 0;
                        _context.Update(NewVacation);
                        await _context.SaveChangesAsync();

                    }
                    vacation.FromDate = NewVacation.DailyCreditDate;
                    DateOnly tempDate = (DateOnly)vacation.FromDate!;
                    vacation.NoOfDays = (tempDate.AddMonths(1).DayNumber - tempDate.DayNumber);
                    vacation.Emp = null;
                    _context.Update(vacation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(vacation);
        }

        // GET: Vacations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacation == null)
            {
                return NotFound();
            }

            return View(vacation);
        }

        // POST: Vacations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation != null)
            {
                _context.Vacations.Remove(vacation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VacationExists(int id)
        {
            return _context.Vacations.Any(e => e.Id == id);
        }
    }
}
