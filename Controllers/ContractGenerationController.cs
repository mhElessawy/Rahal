using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;
using IO = System.IO;

public class ContractGenerationController : Controller
{
    private readonly RahalWebContext _context;
    private readonly WordDocumentService _wordService;

    public ContractGenerationController(RahalWebContext context, IWebHostEnvironment env)
    {
        _context = context;
        _wordService = new WordDocumentService(context, env.ContentRootPath);
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET: Quick search by contract number
    public IActionResult QuickByContractNo(string contractNo)
    {
        if (string.IsNullOrEmpty(contractNo))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال رقم العقد";
            return RedirectToAction("Index");
        }

        var contract = _context.Contracts
            .FirstOrDefault(c => c.ContractNo == contractNo && c.DeleteFlag == 0);

        if (contract == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على عقد بهذا الرقم";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Generate", new { id = contract.Id });
    }

    // GET: Quick search by employee code -> list the employee's contracts
    public IActionResult QuickByEmployeeCode(string empCode)
    {
        if (string.IsNullOrEmpty(empCode))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال كود الموظف";
            return RedirectToAction("Index");
        }

        var employee = _context.EmployeeInfos
            .FirstOrDefault(e => e.EmpCode.ToString() == empCode && e.DeleteFlag != 1);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على موظف بهذا الكود";
            return RedirectToAction("Index");
        }

        return RedirectToAction("EmployeeContracts", new { employeeId = employee.Id });
    }

    // GET: list of an employee's contracts to choose which one to print
    public IActionResult EmployeeContracts(int employeeId)
    {
        var employee = _context.EmployeeInfos.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            return NotFound();

        var contracts = _context.Contracts
            .Include(c => c.Car)
            .Where(c => c.EmployeeId == employeeId && c.DeleteFlag == 0)
            .OrderByDescending(c => c.ContractDate)
            .ToList();

        ViewBag.Employee = employee;
        return View(contracts);
    }

    // GET: Generate the contract document for a specific contract
    public IActionResult Generate(int id)
    {
        try
        {
            var documentBytes = _wordService.GenerateContractDocument(id);

            var contract = _context.Contracts
                .Include(c => c.Employee)
                .FirstOrDefault(c => c.Id == id);

            string fileName = $"عقد_{contract?.Employee?.FullNameAr}_{DateTime.Now:yyyyMMdd}.docx";
            fileName = string.Join("_", fileName.Split(IO.Path.GetInvalidFileNameChars()));

            return File(documentBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"خطأ في إنشاء المستند: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // AJAX: employee suggestions for the autocomplete search box
    [HttpGet]
    public JsonResult GetEmployeeSuggestions(string term)
    {
        if (string.IsNullOrEmpty(term))
            return Json(new List<object>());

        var employees = _context.EmployeeInfos
            .Where(e => e.DeleteFlag != 1 &&
                        (e.FullNameAr.Contains(term) ||
                         e.FullNameEn.Contains(term) ||
                         e.CivilId.Contains(term) ||
                         e.EmpCode.ToString().Contains(term)))
            .Select(e => new
            {
                id = e.Id,
                text = $"{e.FullNameAr} - {e.CivilId} - {e.EmpCode}"
            })
            .Take(10)
            .ToList();

        return Json(employees);
    }
}
