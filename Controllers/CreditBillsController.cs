using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class CreditBillsController : Controller
    {
        private readonly RahalWebContext _context;

        public CreditBillsController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: CreditBills
        public async Task<IActionResult> Index(string? CarNoSearch, int? EmpNoSearch, string? ContractNoSearch, int? companyId, int? pageNumber, DateTime? FromDateSearch, DateTime? ToDateSearch, string? EmpNameSearch)
        {
            ViewBag.Companies = new SelectList(
             await _context.CompanyInfos
                 .Where(c => c.DeleteFlag == 0)
                 .OrderBy(c => c.CompNameAr)
                 .ToListAsync(),
             "Id",
             "CompNameAr",
             companyId);

            var query = _context.CreditBills
                .Include(c => c.Employee)
                .Include(c => c.Contract)
                .Include(c => c.Contract!.Car)
                .Include(c => c.User)
                .Where(a => a.Contract!.ContractType == 1);

            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.Contract!.Car!.CarNo!.Contains(CarNoSearch));
            }

            if (EmpNoSearch.HasValue)
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.Employee!.EmpCode == EmpNoSearch);
            }

            if (!string.IsNullOrEmpty(EmpNameSearch))
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.Employee!.FullNameAr!.Contains(EmpNameSearch));
            }


            if (!string.IsNullOrEmpty(ContractNoSearch))
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.Contract!.ContractNo!.Contains(ContractNoSearch));
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.Employee!.CompanyId == companyId.Value);
            }

            if (FromDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.CreditBillDate >= DateOnly.FromDateTime((DateTime)FromDateSearch));

            }
            if (ToDateSearch.HasValue)
            {
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.CreditBillDate <= DateOnly.FromDateTime((DateTime)ToDateSearch));
            }

            if (FromDateSearch == null && ToDateSearch == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysAgo = today.AddDays(-7); // Subtract 7 days
                query = (IOrderedQueryable<CreditBill>)query.Where(e => e.CreditBillDate >= sevenDaysAgo);
            }

            // Store current search values for the view
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["EmpNoFilter"] = EmpNoSearch;
            ViewData["ContractNoFilter"] = ContractNoSearch;
            ViewData["CompanyFilter"] = companyId;

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
            // Convert to string

            @ViewData["EmpNameFilter"] = EmpNameSearch;

            decimal? totalBillPayed = (decimal?)query.Sum(item => item.CreditBillPayed);

            ViewBag.TotalBillPayed = totalBillPayed;

            // Pagination
            int pageSize = 10; // Set your page size
            return View(await PaginatedList<CreditBill>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: CreditBills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var creditBill = await _context.CreditBills
                .Include(c => c.BankIntNoNavigation)
                .Include(c => c.Contract)
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (creditBill == null)
            {
                return NotFound();
            }

            return View(creditBill);
        }

        // GET: CreditBills/Create
        public IActionResult Create()
        {
            int maxBillNo = _context.CreditBills.Any()
                            ? _context.CreditBills.Max(a => a.CreditBillNo ?? 0)
                            : 0;

            ViewBag.maxBillNo = maxBillNo + 1;

            var employeesWithContracts = _context.EmployeeInfos.Where(employee =>
                                         _context.Contracts.Any(contract => contract.EmployeeId == employee.Id && contract.DeleteFlag == 0 && contract.Status == 0 && contract.ContractType == 1 && contract.CreditTotalCost != 0))
                                        .OrderBy(a => a.FullNameAr)
                                        .ToList();

            var bank = _context.Deffs.Where(a => a.DeffType == 9).ToList();

            ViewBag.BankId = new SelectList(bank, "Id", "DeffName");
            ViewBag.EmployeeId = new SelectList(employeesWithContracts, "Id", "FullNameAr");

            return View();
        }

        // POST: CreditBills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreditBill creditBill)
        {
            if (ModelState.IsValid)
            {

                if (creditBill.BankBillNo == null)
                {
                    creditBill.BankIntNo = 568;
                }

                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
                int daysDifference = currentDate.Month - creditBill.ToDate!.Value.Month;
                if (daysDifference > 0)
                {
                    creditBill.LateMonth = daysDifference;
                }
                else
                {
                    creditBill.LateMonth = 0;
                }

                creditBill.CreditBillTime = TimeOnly.FromDateTime(DateTime.Now);
                var contractid = _context.Contracts.Where(a => a.EmployeeId == creditBill.EmployeeId && a.DeleteFlag == 0 && a.Status == 0)
                      .Select(a => a.Id)
                      .FirstOrDefault(); // or .SingleOrDefault() if you expect exactly one
                creditBill.ContractId = contractid;

                _context.Add(creditBill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(creditBill);
        }

        // GET: CreditBills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var creditBill = await _context.CreditBills.FindAsync(id);
            if (creditBill == null)
            {
                return NotFound();
            }
            ViewData["BankIntNo"] = new SelectList(_context.Deffs, "Id", "Id", creditBill.BankIntNo);
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", creditBill.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", creditBill.EmployeeId);
            return View(creditBill);
        }

        // POST: CreditBills/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreditBillNo,EmployeeId,FromDate,ToDate,NoOfMonth,CreditBillDate,CreditBillTime,CreditBillPayed,LateMonth,ContractId,DeleteFlag,UserRecievedId,UserRecievedDate,BankIntNo,BankBillNo,BankDate,BillHent,DeleteReson,UserId")] CreditBill creditBill)
        {
            if (id != creditBill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(creditBill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreditBillExists(creditBill.Id))
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
            ViewData["BankIntNo"] = new SelectList(_context.Deffs, "Id", "Id", creditBill.BankIntNo);
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", creditBill.ContractId);
            ViewData["EmployeeId"] = new SelectList(_context.EmployeeInfos, "Id", "Id", creditBill.EmployeeId);
            return View(creditBill);
        }

        // GET: CreditBills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var creditBill = await _context.CreditBills
                .Include(c => c.BankIntNoNavigation)
                .Include(c => c.Contract)
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (creditBill == null)
            {
                return NotFound();
            }

            return View(creditBill);
        }

        // POST: CreditBills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var creditBill = await _context.CreditBills.FindAsync(id);
            if (creditBill != null)
            {
                _context.CreditBills.Remove(creditBill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CreditBillExists(int id)
        {
            return _context.CreditBills.Any(e => e.Id == id);
        }


        public IActionResult GetLastBillForEmployeeMonthly(int employeeId)
        {
            // Query your database to get the last bill number for this employee
            var lastBillNumber = _context.CreditBills
                .Where(b => b.EmployeeId == employeeId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            var billCost = _context.Contracts
                .Where(a => a.EmployeeId == employeeId && a.DeleteFlag == 0 && a.Status == 0)
                .FirstOrDefault(); // Use FirstOrDefault instead of ToList since we only need the first record

            if (lastBillNumber != null)
            {
                // If we have a bill, return it along with the contract data
                return Json(new
                {
                    billNo = lastBillNumber.CreditBillNo,
                    fromDate = lastBillNumber.FromDate,
                    toDate = lastBillNumber.ToDate,
                    billDate = lastBillNumber.CreditBillDate,
                    billPayed = lastBillNumber.CreditBillPayed,
                    noOfDays = lastBillNumber.NoOfMonth,
                    dailyMonth = billCost?.CreditMonthPay,
                    startDate = billCost?.CreditStartDate
                });
            }
            else if (billCost != null)
            {
                // If no bill but we have contract data, return just the contract data
                return Json(new
                {
                    dailyMonth = billCost.CreditMonthPay,
                    startDate = billCost.CreditStartDate
                });
            }

            // If neither bill nor contract data exists
            return Json(null);
        }


    }
}
