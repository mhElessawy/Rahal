using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class CompanyDebitsController : Controller
    {
        private readonly RahalWebContext _context;

        public CompanyDebitsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: CompanyDebits
        public async Task<IActionResult> Index(int? EmpCodeString, string? EmpSearch, DateTime? FromDateSearch, DateTime? ToDateSearch)
        {

            // Get companies for dropdown
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            // Get companies for dropdown
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var query = _context.CompanyDebits.Include(u=>u.UserInfo).Include(e=>e.Employee).OrderByDescending(b => b.Id);
            if (EmpCodeString.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebit>)query.Where(e => e.Employee!.EmpCode == EmpCodeString);
            }

            if (!string.IsNullOrEmpty(EmpSearch))
            {
                query = (IOrderedQueryable<CompanyDebit>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpSearch));
            }


            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebit>)query.Where(e => e.DebitDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CompanyDebit>)query.Where(e => e.DebitDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }
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
            ViewData["EmpCodeFilter"] = EmpCodeString;
            ViewData["EmpFilter"] = EmpSearch;


            return View(await query.ToListAsync());
        }



        public async Task<IActionResult> IndexPrint()
        {
            var rahalWebContext = _context.CompanyDebits.Include(u => u.UserInfo).Include(e => e.Employee).OrderByDescending(b => b.Id);
            return View(await rahalWebContext.ToListAsync());
        }


        // GET: CompanyDebits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebit = await _context.CompanyDebits
                .Include(c => c.Employee)
                .Include(c => c.UserInfo)
                .Include(c => c.UserInfoRecieve)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyDebit == null)
            {
                return NotFound();
            }

            return View(companyDebit);
        }

        // GET: CompanyDebits/Create
        public IActionResult Create()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            int maxCompDebitNo = _context.CompanyDebits.Max(a => Convert.ToInt32(a.CompDebitNo));
            ViewBag.maxCompDebitNo = maxCompDebitNo + 1;



            return View();
        }

        // POST: CompanyDebits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CompanyDebit companyDebit, int maxCompDebitNo)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            if (ModelState.IsValid)
            {
                maxCompDebitNo = _context.CompanyDebits.Max(a => Convert.ToInt32(a.CompDebitNo));
                companyDebit.CompDebitNo = maxCompDebitNo + 1;
                companyDebit.ReminderQty = companyDebit.DebitQty;
                companyDebit.UserId = (int)ViewData["UserId"];
                companyDebit.Employee = null;
                _context.Add(companyDebit);
                await _context.SaveChangesAsync();

                try
                {
                    int CompDebitId = _context.CompanyDebits.Any() ?
                        _context.CompanyDebits.Max(a => Convert.ToInt32(a.Id)) : 0;


                    int maxCompDebitDetailsNo = _context.CompanyDebitDetails.Max(a => Convert.ToInt32(a.CompDebitDetailsNo));

                    //int maxCompDebitDetailsNo = _context.CompanyDebitDetails.Any() ?
                    //    _context.CompanyDebitDetails.Max(a => Convert.ToInt32(a.CompDebitDetailsNo)) : 0;
                    maxCompDebitDetailsNo++;

                    var newContractDebitDetails = new CompanyDebitDetails
                    {
                        CompDebitId = CompDebitId,
                        CompDebitDetailsNo = maxCompDebitDetailsNo,
                        CompDebitPayed = companyDebit.DebitQty,
                        CompDebitDate = companyDebit.DebitDate,
                        CompDebitType = 1,
                        UserId = (int)ViewData["UserId"],
                        UserRecievedId = (int)ViewData["UserId"],
                        UserRecievedDate = DateOnly.FromDateTime(DateTime.Now) // Add this
                    };

                    _context.CompanyDebitDetails.Add(newContractDebitDetails);

                    await _context.SaveChangesAsync();

                    Console.WriteLine("Save successful!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                }

                return RedirectToAction(nameof(Index));
            }
           
            return View(companyDebit);
        }

        // GET: CompanyDebits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebit = await _context.CompanyDebits.FindAsync(id);
            if (companyDebit == null)
            {
                return NotFound();
            }
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", companyDebit.EmpId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebit.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebit.UserRecievedId);
            return View(companyDebit);
        }

        // POST: CompanyDebits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  CompanyDebit companyDebit)
        {
            if (id != companyDebit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    companyDebit.UserId = (int)HttpContext.Session.GetInt32("UserId");

                    companyDebit.ReminderQty = companyDebit.DebitQty - companyDebit.PayedQty;
                   

                    _context.Update(companyDebit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyDebitExists(companyDebit.Id))
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
            ViewData["EmpId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", companyDebit.EmpId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebit.UserId);
            ViewData["UserRecievedId"] = new SelectList(_context.PasswordData, "Id", "Id", companyDebit.UserRecievedId);
            return View(companyDebit);
        }

        // GET: CompanyDebits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyDebit = await _context.CompanyDebits
                .Include(c => c.Employee)
                .Include(c => c.UserInfo)
                .Include(c => c.UserInfoRecieve)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyDebit == null)
            {
                return NotFound();
            }

            return View(companyDebit);
        }

        // POST: CompanyDebits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyDebit = await _context.CompanyDebits.FindAsync(id);
            if (companyDebit != null)
            {
                var compDebitDetails = await _context.CompanyDebitDetails
                    .Where(a => a.CompDebitId == id)
                    .ToListAsync();

                _context.CompanyDebitDetails.RemoveRange(compDebitDetails);

                _context.CompanyDebits.Remove(companyDebit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyDebitExists(int id)
        {
            return _context.CompanyDebits.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeByCode(int empCode)
        {
            try
            {
                // Query your database for the employee
                var employee = await _context.EmployeeInfos
                    .Where(e => e.EmpCode == empCode && e.DeleteFlag == 0)
                    .Select(e => new { e.Id, e.FullNameAr })
                    .FirstOrDefaultAsync();

                if (employee != null)
                {
                    return Ok(new
                    {
                        success = true,
                        id = employee.Id,
                        fullNameAr = employee.FullNameAr
                    });
                }

                return Ok(new { success = false });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false });
            }
        }

        public async Task<IActionResult> Pay(int Id)
        {

            int? maxCompanyDebitDetailsNo = (int?)_context.CompanyDebitDetails.Max(a => (int?)a.CompDebitDetailsNo);
            ViewBag.MaxCompanyDebitDetailsNo = maxCompanyDebitDetailsNo + 1 ?? 1;

            // Use FirstOrDefaultAsync to get a single CompanyDebit object, not IQueryable
            var compDebit = await _context.CompanyDebits
                .Where(a => a.Id == Id)
                .Include(c => c.CompanyDebitsDetails)
                .Include(e => e.Employee)
                .FirstOrDefaultAsync();

            if (compDebit == null)
            {
                return NotFound();
            }

            return View(compDebit);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, decimal DebitPayQty, int CompDebitDetailsNo)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["UserId"] = userId;
            TempData.Keep();

            // Get the company debit from database
            var existingDebitInfo = await _context.CompanyDebits
                .Include(d => d.CompanyDebitsDetails)
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDebitInfo == null)
            {
                return NotFound();
            }

            // Custom validation checks
            if (DebitPayQty <= 0)
            {
                ModelState.AddModelError("DebitPayQty", "المبلغ يجب أن يكون أكبر من الصفر");
            }

            if (DebitPayQty > existingDebitInfo.ReminderQty)
            {
                ModelState.AddModelError("DebitPayQty", "المدفوع لا يمكن أن يكون أكبر من المبلغ المتبقي");
            }

            // Only proceed if ModelState is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the company debit
                    existingDebitInfo.ReminderQty -= DebitPayQty;
                    existingDebitInfo.PayedQty += DebitPayQty;

                    // Create new debit details record
                    var newContractDebitDetails = new CompanyDebitDetails
                    {
                        CompDebitId = existingDebitInfo.Id,
                        CompDebitDetailsNo = CompDebitDetailsNo,
                        CompDebitPayed = DebitPayQty,
                        CompDebitDate = DateOnly.FromDateTime(DateTime.Now),
                        CompDebitType = 2, // Payed
                        UserId = userId.Value,
                        UserRecievedId = userId.Value,
                        UserRecievedDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.CompanyDebitDetails.Add(newContractDebitDetails);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    ModelState.AddModelError("", "حدث خطأ أثناء حفظ البيانات");
                }
            }

            // If we get here, something went wrong - reload the ViewBag and return the view
            int? maxCompanyDebitDetailsNo = (int?)_context.CompanyDebitDetails.Max(a => (int?)a.CompDebitDetailsNo);
            ViewBag.MaxCompanyDebitDetailsNo = maxCompanyDebitDetailsNo + 1 ?? 1;

            return View(existingDebitInfo);
        }



    }
}
