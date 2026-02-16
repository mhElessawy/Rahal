using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;

namespace RahalWeb.Controllers
{
    public class EmpTreatmentsController : Controller
    {
        private readonly RahalWebContext _context;

        public EmpTreatmentsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: EmpTreatments
        public async Task<IActionResult> Index()
        {
            TempData.Keep();
            var data = _context.EmpTreatments
                .Include(d => d.Emp)
                .Include(d => d.DeffEmpTreatment)
                .Include(d => d.User)
                .Where(d => d.DeleteFlag == 0)
                .OrderByDescending(d => d.Date);
            return View(await data.ToListAsync());
        }

        // GET: EmpTreatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.EmpTreatments
                .Include(d => d.Emp)
                .Include(d => d.DeffEmpTreatment)
                .Include(d => d.User)
                .Include(d => d.UserRecieved)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: EmpTreatments/Create
        public IActionResult Create()
        {
            TempData.Keep();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr");
            ViewBag.DeffEmpTreatmentId = new SelectList(_context.DeffEmpTreatments.Where(d => d.DeleteFlag == 0), "Id", "DeffName");
            int maxNo = _context.EmpTreatments.Any() ? _context.EmpTreatments.Max(a => a.TreatmentNo ?? 0) : 0;
            ViewBag.maxTreatmentNo = maxNo + 1;
            return View();
        }

        // POST: EmpTreatments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpTreatment empTreatment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", empTreatment.EmpId);
            ViewBag.DeffEmpTreatmentId = new SelectList(_context.DeffEmpTreatments.Where(d => d.DeleteFlag == 0), "Id", "DeffName", empTreatment.DeffEmpTreatmentId);
            return View(empTreatment);
        }

        // GET: EmpTreatments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TempData.Keep();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            var item = await _context.EmpTreatments.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", item.EmpId);
            ViewBag.DeffEmpTreatmentId = new SelectList(_context.DeffEmpTreatments.Where(d => d.DeleteFlag == 0), "Id", "DeffName", item.DeffEmpTreatmentId);
            return View(item);
        }

        // POST: EmpTreatments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpId,DeffEmpTreatmentId,TreatmentNo,Date,TreatmentDetails,TreatmentExtraMoney,TreatmentTotal,DeleteFlag,UserId,UserRecievedId,UserRecievedDate,UserRecievedNo")] EmpTreatment empTreatment)
        {
            if (id != empTreatment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpTreatmentExists(empTreatment.Id))
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
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", empTreatment.EmpId);
            ViewBag.DeffEmpTreatmentId = new SelectList(_context.DeffEmpTreatments.Where(d => d.DeleteFlag == 0), "Id", "DeffName", empTreatment.DeffEmpTreatmentId);
            return View(empTreatment);
        }

        // GET: EmpTreatments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.EmpTreatments
                .Include(d => d.Emp)
                .Include(d => d.DeffEmpTreatment)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: EmpTreatments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.EmpTreatments.FindAsync(id);
            if (item != null)
            {
                _context.EmpTreatments.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpTreatmentExists(int id)
        {
            return _context.EmpTreatments.Any(e => e.Id == id);
        }

        // API: Get employee by code
        [HttpGet]
        public async Task<IActionResult> GetEmployeeByCode(int code)
        {
            var emp = await _context.EmployeeInfos
                .Where(e => e.EmpCode == code && e.DeleteFlag == 0)
                .Select(e => new { e.Id, e.FullNameAr })
                .FirstOrDefaultAsync();
            if (emp == null)
                return Json(new { success = false });
            return Json(new { success = true, id = emp.Id, name = emp.FullNameAr });
        }

        // API: Get treatment prices by DeffEmpTreatment id
        [HttpGet]
        public async Task<IActionResult> GetTreatmentPrices(int id)
        {
            var item = await _context.DeffEmpTreatments
                .Where(d => d.Id == id)
                .Select(d => new { d.Price1, d.Price2, d.Price3 })
                .FirstOrDefaultAsync();
            if (item == null)
                return Json(new { success = false });
            return Json(new { success = true, price1 = item.Price1, price2 = item.Price2, price3 = item.Price3 });
        }
    }
}