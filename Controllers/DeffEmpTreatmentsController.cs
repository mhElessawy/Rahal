using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;

namespace RahalWeb.Controllers
{
    public class DeffEmpTreatmentsController : Controller
    {
        private readonly RahalWebContext _context;

        public DeffEmpTreatmentsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: DeffEmpTreatments
        public async Task<IActionResult> Index()
        {
            TempData.Keep();
            var data = _context.DeffEmpTreatments
                .Include(d => d.Employee)
                .Include(d => d.DeffTreatment)
                .Include(d => d.User)
                .Where(d => d.DeleteFlag == 0)
                .OrderByDescending(d => d.TreatmentDate);
            return View(await data.ToListAsync());
        }

        // GET: DeffEmpTreatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.DeffEmpTreatments
                .Include(d => d.Employee)
                .Include(d => d.DeffTreatment)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: DeffEmpTreatments/Create
        public IActionResult Create()
        {
            TempData.Keep();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr");
            ViewBag.DeffId = new SelectList(_context.Deffs.Where(d => d.DeleteFlag == 0), "Id", "DeffName");
            int maxNo = _context.DeffEmpTreatments.Any() ? _context.DeffEmpTreatments.Max(a => a.TreatmentNo) : 0;
            ViewBag.maxTreatmentNo = maxNo + 1;
            return View();
        }

        // POST: DeffEmpTreatments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeffEmpTreatment deffEmpTreatment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deffEmpTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", deffEmpTreatment.EmpId);
            ViewBag.DeffId = new SelectList(_context.Deffs.Where(d => d.DeleteFlag == 0), "Id", "DeffName", deffEmpTreatment.DeffId);
            return View(deffEmpTreatment);
        }

        // GET: DeffEmpTreatments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TempData.Keep();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            var item = await _context.DeffEmpTreatments.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", item.EmpId);
            ViewBag.DeffId = new SelectList(_context.Deffs.Where(d => d.DeleteFlag == 0), "Id", "DeffName", item.DeffId);
            return View(item);
        }

        // POST: DeffEmpTreatments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpId,DeffId,TreatmentDate,TreatmentAmount,Notes,DeleteFlag,UserId,TreatmentNo")] DeffEmpTreatment deffEmpTreatment)
        {
            if (id != deffEmpTreatment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deffEmpTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeffEmpTreatmentExists(deffEmpTreatment.Id))
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
            ViewBag.EmpId = new SelectList(_context.EmployeeInfos.Where(e => e.DeleteFlag == 0), "Id", "FullNameAr", deffEmpTreatment.EmpId);
            ViewBag.DeffId = new SelectList(_context.Deffs.Where(d => d.DeleteFlag == 0), "Id", "DeffName", deffEmpTreatment.DeffId);
            return View(deffEmpTreatment);
        }

        // GET: DeffEmpTreatments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.DeffEmpTreatments
                .Include(d => d.Employee)
                .Include(d => d.DeffTreatment)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: DeffEmpTreatments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.DeffEmpTreatments.FindAsync(id);
            if (item != null)
            {
                _context.DeffEmpTreatments.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeffEmpTreatmentExists(int id)
        {
            return _context.DeffEmpTreatments.Any(e => e.Id == id);
        }
    }
}
