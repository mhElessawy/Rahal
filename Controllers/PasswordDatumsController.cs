using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class PasswordDatumsController : Controller
    {

        private readonly RahalWebContext _context;

        public PasswordDatumsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: PasswordDatums
        public async Task<IActionResult> Index()
        {
            return View(await _context.PasswordData.ToListAsync());
        }

        // GET: PasswordDatums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passwordDatum = await _context.PasswordData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (passwordDatum == null)
            {
                return NotFound();
            }

            return View(passwordDatum);
        }

        // GET: PasswordDatums/Create
        public IActionResult Create()
        {
            var companies = _context.CompanyInfos
                     .Where(c => c.DeleteFlag == 0)
                     .Select(c => new SelectListItem
                     {
                         Value = c.Id.ToString(),
                         Text = c.CompNameAr
                     })
                     .ToList();

            ViewBag.Companies = companies;

            return View();
        }

        // POST: PasswordDatums/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PasswordDatum passwordDatum , List<string> SelectedCompanies)
        {
            if (ModelState.IsValid)
            {

                var loginExist = _context.PasswordData.Where(a => a.UserName == passwordDatum.UserName && a.DeleteFlag ==0).ToList();
                if (loginExist.Count  != 0)
                {
                    ModelState.AddModelError("UserName", "اسم المستخدم موجود مسبقا ");
                    return View(passwordDatum);

                }

                passwordDatum.CompanyData = (SelectedCompanies == null || !SelectedCompanies.Any())
                                        ? "0"
                                        : string.Join(",", SelectedCompanies);


                passwordDatum.EmpId = 0;
                string hashedPassword = HashPassword(passwordDatum.Password!);
                passwordDatum.Password = hashedPassword; // Store the hashed versi
                _context.Add(passwordDatum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(passwordDatum);
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetInt32("UserId", 0);
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewData["ErrorMessage"] = "اسم المستخدم وكلمة المرور مطلوبان";
                return View();
            }

            var user = _context.PasswordData.FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                ViewData["ErrorMessage"] = "بيانات الدخول غير صحيحة";
                return View();
            }

            if (!VerifyPassword(user.Password!, password))
            {
                ViewData["ErrorMessage"] = "بيانات الدخول غير صحيحة";
                return View();
            }

            // Login successful - create auth session
            HttpContext.Session.SetString("Username", user.UserName!);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserCompanyData", user.CompanyData ?? "0");
            HttpContext.Session.SetString("PurshaseShowAll", user.PurshaseShowAll.ToString());

            return RedirectToAction("Index", "Home");
        }

        // Password verification (matches your hashing approach)
        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(null!, password);
        }

        // Your verification should use this:
        public bool VerifyPassword(string storedHash, string providedPassword)
        {
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(null!, storedHash, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        // GET: PasswordDatums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get all companies
            var companies = _context.CompanyInfos
                    .Where(c => c.DeleteFlag == 0)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CompNameAr
                    })
                    .ToList();

            // Get the password record
            var passwordDatum = await _context.PasswordData.FindAsync(id);
            if (passwordDatum == null)
            {
                return NotFound();
            }

            // If CompanyData contains comma-separated company IDs
            if (!string.IsNullOrEmpty(passwordDatum.CompanyData))
            {
                var selectedCompanyIds = passwordDatum.CompanyData.Split(',');

                // Mark the appropriate companies as selected
                foreach (var company in companies)
                {
                    if (selectedCompanyIds.Contains(company.Value))
                    {
                        company.Selected = true;
                    }
                }
            }

            ViewBag.Companies = companies;
            return View(passwordDatum);
        }

        // POST: PasswordDatums/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PasswordDatum passwordDatum, List<string> SelectedCompanies)
        {
            if (id != passwordDatum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    passwordDatum.CompanyData = (SelectedCompanies == null || !SelectedCompanies.Any())
                                        ? "0"
                                        : string.Join(",", SelectedCompanies);
                    _context.Update(passwordDatum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PasswordDatumExists(passwordDatum.Id))
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
            return View(passwordDatum);
        }

        // GET: PasswordDatums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passwordDatum = await _context.PasswordData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (passwordDatum == null)
            {
                return NotFound();
            }

            return View(passwordDatum);
        }

        // POST: PasswordDatums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var passwordDatum = await _context.PasswordData.FindAsync(id);
            if (passwordDatum != null)
            {
                _context.PasswordData.Remove(passwordDatum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PasswordDatumExists(int id)
        {
            return _context.PasswordData.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passwordDatum = await _context.PasswordData.FindAsync(id);
            if (passwordDatum == null)
            {
                return NotFound();
            }
            return View(passwordDatum);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int? id , PasswordDatum passwordDatum)
        {
            if (id != passwordDatum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    passwordDatum.DeleteFlag = 0;
                    passwordDatum.EmpId = 0;
                    string hashedPassword = HashPassword(passwordDatum.Password!);
                    passwordDatum.Password = hashedPassword; // Store the hashed versi
                    _context.Update(passwordDatum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PasswordDatumExists(passwordDatum.Id))
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
            return View(passwordDatum);
        }
    }
}
