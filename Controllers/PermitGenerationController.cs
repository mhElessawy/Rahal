using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;
using IO = System.IO;


public class PermitGenerationController : Controller
{
    private readonly RahalWebContext  _context;
    private readonly WordDocumentService _wordService;

    public PermitGenerationController(RahalWebContext context)
    {
        _context = context;
        _wordService = new WordDocumentService(context);
    }

    public IActionResult Index()
    {
        ViewBag.DocumentType = "permit";
        return View();
    }

    public IActionResult IndexRenewal()
    {
        ViewBag.DocumentType = "renewal";
        return View("Index");
    }

    // GET: Search page
    public IActionResult Search(string documentType = "permit")
    {
        ViewBag.DocumentType = documentType;
        return View();
    }

    // POST: Search for employees
    [HttpPost]
    public IActionResult Search(string searchTerm, string searchType, string documentType = "permit")
    {
        IQueryable<EmployeeInfo> query = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .Where(e => e.DeleteFlag != 1);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            switch (searchType)
            {
                case "code":
                    if (int.TryParse(searchTerm, out int empCode))
                        query = query.Where(e => e.EmpCode == empCode);
                    break;

                case "civilId":
                    query = query.Where(e => e.CivilId.Contains(searchTerm));
                    break;

                case "name":
                    query = query.Where(e => e.FullNameAr.Contains(searchTerm) ||
                                             e.FullNameEn.Contains(searchTerm));
                    break;

                default:
                    // Search in all fields
                    query = query.Where(e => e.FullNameAr.Contains(searchTerm) ||
                                             e.FullNameEn.Contains(searchTerm) ||
                                             e.CivilId.Contains(searchTerm) ||
                                             e.EmpCode.ToString().Contains(searchTerm));
                    break;
            }
        }

        var employees = query.OrderBy(e => e.FullNameAr)
                            .Take(100)
                            .ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.SearchType = searchType;
        ViewBag.DocumentType = documentType;

        return View("SearchResults", employees);
    }

    // GET: Quick search by employee code
    public IActionResult QuickByCode(string empCode)
    {
        if (string.IsNullOrEmpty(empCode))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال كود الموظف";
            return RedirectToAction("Index");
        }

        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .FirstOrDefault(e => e.EmpCode.ToString() == empCode);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على موظف بهذا الكود";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Generate", new { id = employee.Id });
    }

    // GET: Quick search by civil ID
    public IActionResult QuickByCivilId(string civilId)
    {
        if (string.IsNullOrEmpty(civilId))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال الرقم المدني";
            return RedirectToAction("Index");
        }

        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .FirstOrDefault(e => e.CivilId == civilId);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على موظف بهذا الرقم المدني";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Generate", new { id = employee.Id });
    }

    // GET: Quick search by employee code for renewal
    public IActionResult QuickByCodeRenewal(string empCode)
    {
        if (string.IsNullOrEmpty(empCode))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال كود الموظف";
            return RedirectToAction("IndexRenewal");
        }

        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .FirstOrDefault(e => e.EmpCode.ToString() == empCode);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على موظف بهذا الكود";
            return RedirectToAction("IndexRenewal");
        }

        return RedirectToAction("GenerateRenewal", new { id = employee.Id });
    }

    // GET: Quick search by civil ID for renewal
    public IActionResult QuickByCivilIdRenewal(string civilId)
    {
        if (string.IsNullOrEmpty(civilId))
        {
            TempData["ErrorMessage"] = "الرجاء إدخال الرقم المدني";
            return RedirectToAction("IndexRenewal");
        }

        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .FirstOrDefault(e => e.CivilId == civilId);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "لم يتم العثور على موظف بهذا الرقم المدني";
            return RedirectToAction("IndexRenewal");
        }

        return RedirectToAction("GenerateRenewal", new { id = employee.Id });
    }

    // GET: Generate document for specific employee
    public IActionResult Generate(int id)
    {
        try
        {
            var documentBytes = _wordService.GeneratePermitDocument(id);

            // Get employee name for file name
            var employee = _context.EmployeeInfos.Find(id);
            string fileName = $"تصريح_إجرة_{employee?.FullNameAr}_{DateTime.Now:yyyyMMdd}.docx";

            // Clean file name
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

    // GET: Generate renewal document for specific employee
    public IActionResult GenerateRenewal(int id)
    {
        try
        {
            var documentBytes = _wordService.GenerateRenewalDocument(id);

            // Get employee name for file name
            var employee = _context.EmployeeInfos.Find(id);
            string fileName = $"تجديد_رخصة_{employee?.FullNameAr}_{DateTime.Now:yyyyMMdd}.doc";

            // Clean file name
            fileName = string.Join("_", fileName.Split(IO.Path.GetInvalidFileNameChars()));

            return File(documentBytes,
                "application/msword",
                fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"خطأ في إنشاء المستند: {ex.Message}";
            return RedirectToAction("IndexRenewal");
        }
    }

    // AJAX: Get employee suggestions for autocomplete
    [HttpGet]
    public JsonResult GetEmployeeSuggestions(string term)
    {
        if (string.IsNullOrEmpty(term))
            return Json(new List<object>());

        var employees = _context.EmployeeInfos
            .Where(e => e.FullNameAr.Contains(term) ||
                       e.FullNameEn.Contains(term) ||
                       e.CivilId.Contains(term) ||
                       e.EmpCode.ToString().Contains(term))
            .Select(e => new
            {
                id = e.Id,
                text = $"{e.FullNameAr} - {e.CivilId} - {e.EmpCode}",
                nameAr = e.FullNameAr,
                civilId = e.CivilId,
                empCode = e.EmpCode
            })
            .Take(10)
            .ToList();

        return Json(employees);
    }

    // GET: Debug - show template bookmarks
    public IActionResult DebugBookmarks()
    {
        try
        {
            var bookmarks = _wordService.GetTemplateBookmarks();
            ViewBag.Bookmarks = bookmarks;
            return View();
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }

    // GET: Check if template files exist
    public IActionResult CheckTemplate()
    {
        var templatePath1 = IO.Path.Combine(Directory.GetCurrentDirectory(), "Templates", "NewPerm.docx");
        var templatePath2 = IO.Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ReNewPermSp.doc");

        var info = new System.Text.StringBuilder();
        info.AppendLine("Template Files Check:");
        info.AppendLine($"\nNewPerm.docx: {(IO.File.Exists(templatePath1) ? "✓ Found" : "✗ Not Found")}");
        if (IO.File.Exists(templatePath1))
            info.AppendLine($"  Path: {templatePath1}");

        info.AppendLine($"\nReNewPermSp.doc: {(IO.File.Exists(templatePath2) ? "✓ Found" : "✗ Not Found")}");
        if (IO.File.Exists(templatePath2))
            info.AppendLine($"  Path: {templatePath2}");

        return Content(info.ToString(), "text/plain");
    }
}