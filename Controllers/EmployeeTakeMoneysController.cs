using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;
using RahalWeb.Models.MyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class EmployeeTakeMoneysController : Controller
    {
        private readonly RahalWebContext _context;

        public EmployeeTakeMoneysController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: EmployeeTakeMoneys
        public async Task<IActionResult> Index()
        {
            TempData.Keep();
            var rahalWebContext = _context.EmployeeTakeMoney
                                .Include(e => e.TakeUser)
                                .Include(e => e.User)
                                .OrderByDescending(a=>a.TakeDate);
            return View(await rahalWebContext.ToListAsync());
        }

        // GET: EmployeeTakeMoneys/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeTakeMoney = await _context.EmployeeTakeMoney
                .Include(e => e.TakeUser)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeTakeMoney == null)
            {
                return NotFound();
            }

            return View(employeeTakeMoney);
        }

        // GET: EmployeeTakeMoneys/Create
        public IActionResult Create()
        {
            TempData.Keep();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.TakeUserId = new SelectList(_context.PasswordData, "Id", "UserFullName");
            int maxTakeMoneyNo = _context.EmployeeTakeMoney.Max(a => Convert.ToInt32(a.TakeMoneyNo));
            ViewBag.maxTakeMoneyNo = maxTakeMoneyNo + 1;

            return View();
        }

        // POST: EmployeeTakeMoneys/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( EmployeeTakeMoney employeeTakeMoney)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeTakeMoney);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewData["TakeUserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", employeeTakeMoney.TakeUserId);
          
            return View(employeeTakeMoney);
        }

        // GET: EmployeeTakeMoneys/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TempData.Keep();

          
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            var employeeTakeMoney = await _context.EmployeeTakeMoney.FindAsync(id);
            if (employeeTakeMoney == null)
            {
                return NotFound();
            }
            ViewData["TakeUserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", employeeTakeMoney.TakeUserId);
           
            return View(employeeTakeMoney);
        }

        // POST: EmployeeTakeMoneys/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TakeUserId,TakeMoney,TakeDate,DeleteFlag,UserId,TakeMoneyNo")] EmployeeTakeMoney employeeTakeMoney)
        {
            if (id != employeeTakeMoney.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeTakeMoney);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeTakeMoneyExists(employeeTakeMoney.Id))
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
            ViewData["TakeUserId"] = new SelectList(_context.PasswordData, "Id", "Id", employeeTakeMoney.TakeUserId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", employeeTakeMoney.UserId);
            return View(employeeTakeMoney);
        }

        // GET: EmployeeTakeMoneys/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeTakeMoney = await _context.EmployeeTakeMoney
                .Include(e => e.TakeUser)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeTakeMoney == null)
            {
                return NotFound();
            }

            return View(employeeTakeMoney);
        }

        // POST: EmployeeTakeMoneys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeTakeMoney = await _context.EmployeeTakeMoney.FindAsync(id);
            if (employeeTakeMoney != null)
            {
                _context.EmployeeTakeMoney.Remove(employeeTakeMoney);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeTakeMoneyExists(int id)
        {
            return _context.EmployeeTakeMoney.Any(e => e.Id == id);
        }
     public async Task<IActionResult> IndexReportTakeMoney()
        {
            // Get data from database
            var takeMoneyList = await _context.EmployeeTakeMoney
                .Include(x => x.User)
                .ToListAsync();

            var purshaseList = await _context.Purshases
                .ToListAsync();

            // Generate summary
            var summary = GetCombinedUserSummary(takeMoneyList, purshaseList);

            return View(summary);
        }

        public List<UserTakeMoneySummary> GetCombinedUserSummary(
            List<EmployeeTakeMoney> takeMoneyList,
            List<Purshase> purshaseList)
        {
            // Get all unique user IDs from both collections
            var allUserIds = takeMoneyList.Select(x => x.TakeUserId)
                .Union(purshaseList.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value))
                .Distinct();

            var result = new List<UserTakeMoneySummary>();

            foreach (var userId in allUserIds)
            {
                var userTakeMoney = takeMoneyList
                    .Where(x => x.TakeUserId == userId && x.DeleteFlag == 0)
                    .Sum(x => x.TakeMoney);

                var userPurshase = purshaseList
                    .Where(x => x.UserId == userId && x.DeleteFlag == 0 && x.PurshasePayed.HasValue)
                    .Sum(x => x.PurshasePayed ?? 0);

                // Get user name from related entity if available
                var userName = _context.PasswordData
                                .Where(x => x.Id == userId)
                                .Select(x => x.UserName)
                                .FirstOrDefault();

                result.Add(new UserTakeMoneySummary
                {
                    UserId = userId,
                    UserName = userName,
                    TotalTakeMoney = userTakeMoney,
                    TotalPurshase = userPurshase
                });
            }

            return result.OrderBy(x => x.UserId).ToList();
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var takeMoneyList = await _context.EmployeeTakeMoney.ToListAsync();
            var purshaseList = await _context.Purshases.ToListAsync();
            var summary = GetCombinedUserSummary(takeMoneyList, purshaseList);

            // Excel export logic would go here
            return Json(new { message = "Export functionality to be implemented" });
        }

    
    }
}
