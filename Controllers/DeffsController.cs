using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using RahalWeb.Models;

namespace RahalWeb.Controllers
{
    public class DeffsController : Controller
    {
        private readonly RahalWebContext db;

        public DeffsController(RahalWebContext context)
        {
            db = context;
            
        }

        // GET: Deffs
        public  IActionResult Index(int id , string name)
        {
            TempData["DeffTypeId"] = id;


            TempData["DeffName"] = name;
            TempData.Keep("DeffName");
            TempData.Keep("DeffTypeId");
            

            
            var rahalWebContext = db.Deffs.Include(d => d.DeffTypeNavigation).Where(a=> a.DeffType== id).ToList();

            return View(rahalWebContext);
        }

        // GET: Deffs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deff = await db.Deffs
                .Include(d => d.DeffTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deff == null)
            {
                return NotFound();
            }

            return View(deff);
        }

        // GET: Deffs/Create
        public IActionResult Create(int id)
        {
            TempData.Keep("DeffName");
            TempData.Keep("DeffTypeId");

            // Create a list of SelectListItem manually instead of using SelectList
            var selectListItems = new List<SelectListItem>();

            if (TempData["DeffTypeId"] != null && TempData["DeffTypeId"].ToString() == "23")
            {
                var carTypes = db.Deffs.Where(a => a.DeffType == 21).ToList();
                selectListItems = carTypes.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.DeffName
                }).ToList();
            }

            ViewBag.DeffParentItems = selectListItems; // Different name to avoid confusion
            ViewBag.ShowCarTypeSelect = (TempData["DeffTypeId"]?.ToString() == "23");

            ViewData["DeffType"] = new SelectList(db.DeffTypes, "Id", "Name");
            return View();
        }

        // POST: Deffs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DeffName,DeffType,DeffCode,DeffParent,DeleteFlag,DeffNameEng")] Deff deff)
        {
            
            deff.DeffType = (int?)TempData["DeffTypeId"];

            if (ModelState.IsValid)
            {
                db.Add(deff);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = deff.DeffType , name = TempData["DeffName"] }); ; //,deff.DeffType, TempData["DeffName"]);

               
            }
            ViewData["DeffType"] = new SelectList(db.DeffTypes, "Id", "Name", deff.DeffType);
            return View(deff);
        }

        // GET: Deffs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            TempData.Keep("DeffName");
            if (id == null)
            {
                return NotFound();
            }

            var deff = await db.Deffs.FindAsync(id);
            if (deff == null)
            {
                return NotFound();
            }
          
            ViewData["DeffType"] = new SelectList(db.DeffTypes, "Id", "Name", deff.DeffType);
            return View(deff);
        }

        // POST: Deffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DeffName,DeffType,DeffCode,DeffParent,DeleteFlag,DeffNameEng")] Deff deff)
        {
            int? deffTypeId;
           // string deffTypeName;
            if (id != deff.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    deffTypeId = deff.DeffType;
                    

                    db.Update(deff);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeffExists(deff.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), new { id = deffTypeId, name = TempData["DeffName"] }); ; //,deff.DeffType, TempData["DeffName"]);
            }
            ViewData["DeffType"] = new SelectList(db.DeffTypes, "Id", "Id", deff.DeffType);

            return View(deff);
        }

        // GET: Deffs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            TempData.Keep("DeffName");
            TempData.Keep("DeffTypeId");
            if (id == null)
            {
                return NotFound();
            }

            var deff = await db.Deffs
                .Include(d => d.DeffTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deff == null)
            {
                return NotFound();
            }

            return View(deff);
        }

        // POST: Deffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deff = await db.Deffs.FindAsync(id);
            if (deff != null)
            {
                db.Deffs.Remove(deff);
            }

            await db.SaveChangesAsync();
            return  RedirectToAction(nameof(Index), new { id = deff!.DeffType, name = TempData["DeffName"] }); ; //,deff.DeffType, TempData["DeffName"]);
            
            //return RedirectToAction(nameof(Index));
        }

        private bool DeffExists(int id)
        {
            return db.Deffs.Any(e => e.Id == id);
        }
    }
}
