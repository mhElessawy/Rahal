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
            return View(await _context.DeffEmpTreatment.ToListAsync());
        }

        // GET: DeffEmpTreatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deffEmpTreatment = await _context.DeffEmpTreatment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deffEmpTreatment == null)
            {
                return NotFound();
            }

            return View(deffEmpTreatment);
        }

        // GET: DeffEmpTreatments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeffEmpTreatments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DeffCode,DeffName,Price1,Price2,Price3,DeleteFlag")] DeffEmpTreatment deffEmpTreatment)
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

            var deffEmpTreatment = await _context.DeffEmpTreatment.FindAsync(id);
            if (deffEmpTreatment == null)
            {
                return NotFound();
            }
            return View(deffEmpTreatment);
        }

        // POST: DeffEmpTreatments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            var deffEmpTreatment = await _context.DeffEmpTreatment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deffEmpTreatment == null)
            {
                return NotFound();
            }

            return View(deffEmpTreatment);
        }

        // POST: DeffEmpTreatments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deffEmpTreatment = await _context.DeffEmpTreatment.FindAsync(id);
            if (deffEmpTreatment != null)
            {
                _context.DeffEmpTreatment.Remove(deffEmpTreatment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeffEmpTreatmentExists(int id)
        {
            return _context.DeffEmpTreatment.Any(e => e.Id == id);
        }
    }
}
