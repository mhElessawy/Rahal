using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;

namespace RahalWeb.Controllers
{
    public class CompanyDebitDetailsController : Controller
    {
        private readonly RahalWebContext _context;

        public CompanyDebitDetailsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: CompanyDebitDetails
        public async Task<IActionResult> Index(int? EmpCodeString, string? EmpSearch,  DateTime? FromDateSearch, DateTime? ToDateSearch)
        {

            // Get companies for dropdown
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            // Base query with includes
            var query = _context.CompanyDebitDetails
                .Include(c => c.CompanyDebits)
                  .ThenInclude(c=>c!.Employee)
                .Include(c => c.UserInfo).Include(c => c.UserInfoRecieve)
                .Where(m => m.CompDebitType == 2)
                .OrderBy(e => e.Id);


            // Apply filters
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebitDetails>)query.Where(e => e.CompanyDebits!.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<CompanyDebitDetails>)query.Where(e => e.CompanyDebits!.Employee!.FullNameAr!.Contains(EmpSearch));
            }


            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebitDetails>)query.Where(e => e.CompDebitDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebitDetails>)query.Where(e => e.CompDebitDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<CompanyDebitDetails>)query.Where(e => e.CompDebitDate >= sevenDaysAgo);
            }
            // Store current search values for the view
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;
           
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

            decimal? TotalCompDebitPayed = query.Sum(item => item.CompDebitPayed) ?? 0;
            ViewBag.TotalCompDebitPayed = TotalCompDebitPayed;

            // Pagination

            return View(await query.ToListAsync());
        }

        // GET: CompanyDebitDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebitDetails = await _context.CompanyDebitDetails
                .Include(c => c.CompanyDebits)
                .Include(c => c.UserInfo)
                .Include(c => c.UserInfoRecieve)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyDebitDetails == null)
            {
                return NotFound();
            }

            return View(companyDebitDetails);
        }

        // GET: CompanyDebitDetails/Create
        public IActionResult Create()
        {
            ViewData["CompDebitId"] = new SelectList(_context.CompanyDebits, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id");
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id");
            return View();
        }

        // POST: CompanyDebitDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompDebitId,CompDebitDetailsNo,CompDebitPayed,CompDebitDate,CompDebitType,UserId,UserRecievedId,UserRecievedDate")] CompanyDebitDetails companyDebitDetails)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyDebitDetails);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompDebitId"] = new SelectList(_context.CompanyDebits, "Id", "Id", companyDebitDetails.CompDebitId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserRecievedId);
            return View(companyDebitDetails);
        }

        // GET: CompanyDebitDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebitDetails = await _context.CompanyDebitDetails.FindAsync(id);
            if (companyDebitDetails == null)
            {
                return NotFound();
            }
            ViewData["CompDebitId"] = new SelectList(_context.CompanyDebits, "Id", "Id", companyDebitDetails.CompDebitId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserRecievedId);
            return View(companyDebitDetails);
        }

        // POST: CompanyDebitDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompDebitId,CompDebitDetailsNo,CompDebitPayed,CompDebitDate,CompDebitType,UserId,UserRecievedId,UserRecievedDate")] CompanyDebitDetails companyDebitDetails)
        {
            if (id != companyDebitDetails.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyDebitDetails);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyDebitDetailsExists(companyDebitDetails.Id))
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
            ViewData["CompDebitId"] = new SelectList(_context.CompanyDebits, "Id", "Id", companyDebitDetails.CompDebitId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebitDetails.UserRecievedId);
            return View(companyDebitDetails);
        }

        // GET: CompanyDebitDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebitDetails = await _context.CompanyDebitDetails
                .Include(c => c.CompanyDebits)
                .Include(c => c.UserInfo)
                .Include(c => c.UserInfoRecieve)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyDebitDetails == null)
            {
                return NotFound();
            }

            return View(companyDebitDetails);
        }

        // POST: CompanyDebitDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyDebitDetails = await _context.CompanyDebitDetails.FindAsync(id);
            if (companyDebitDetails != null)
            {
                _context.CompanyDebitDetails.Remove(companyDebitDetails);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyDebitDetailsExists(int id)
        {
            return _context.CompanyDebitDetails.Any(e => e.Id == id);
        }
    }
}
