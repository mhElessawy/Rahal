using Microsoft.AspNetCore.Mvc;
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
                .Where(d => d.DeleteFlag == 0)
                .OrderBy(d => d.DeffCode);
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
            var item = await _context.DeffEmpTreatments.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: DeffEmpTreatments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DeffCode,DeffName,Price1,Price2,Price3,DeleteFlag")] DeffEmpTreatment deffEmpTreatment)
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