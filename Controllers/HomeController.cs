using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System.Diagnostics;
using RahalWeb.Models.MyModel;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace RahalWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly RahalWebContext _context;
        public HomeController(RahalWebContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            var today = DateOnly.FromDateTime(DateTime.Now);
            var sevenMonthAgo = today.AddMonths(-3); // Subtract 3 month
            var count = _context.EmployeeInfos.Count(a => a.ResEndDate <= sevenMonthAgo);
            ViewBag.ResignedEmployeeCount = count;
             count = _context.CarInfos.Count(a => a.CarEndLicense <= sevenMonthAgo);
            ViewBag.EndLicenseCarCount = count;

            count = _context.EmployeeInfos.Count(a => a.EndPerm <= sevenMonthAgo);
            ViewBag.EndPermEmpCount = count;

            count = _context.EmployeeInfos.Count(a => a.EndLicense <= sevenMonthAgo);
            ViewBag.EndLicenseEmpCount = count;

            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values
            if (HttpContext.Session.GetInt32("UserId") ==  0)
            {
                return RedirectToAction("Login", "PasswordDatums");
            }

            HttpContext.Session.SetObjectAsJson("CurrentUser", _context.PasswordData.Find(HttpContext.Session.GetInt32("UserId"))!);
            var user = HttpContext.Session.GetObjectFromJson<PasswordDatum>("CurrentUser");

            return View();
        }

      
        public IActionResult PrintData(int id)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var threeMonthsAgo = today.AddMonths(-3);

            if (id == 1)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.ResEndDate <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "ÅäÊåÇÁ ÇáÅÞÇãÇÊ";
                ViewBag.DataType = "EmpRes";
                return View(data);
            }
            else if (id == 2)
            {
                var data = _context.CarInfos
                    .Where(a => a.CarEndLicense <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "ÅäÊåÇÁ ÑÎÕÉ ÓæÇÞ";
                ViewBag.DataType = "CarLicense";
                return View(data);
            }
            else if (id == 3)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.EndPerm <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "ÅäÊåÇÁ ÅÐä ÇáÚãá";
                ViewBag.DataType = "EmpPerm";
                return View(data);
            }
            else if (id == 4)
            {
                var data = _context.EmployeeInfos
                    .Where(a => a.EndLicense <= threeMonthsAgo)
                    .ToList();

                ViewBag.Header = "ÅäÊåÇÁ ÑÎÕÉ ÇáÓæÇÞÉ";
                ViewBag.DataType = "EmpLicense";
                return View(data);
            }

            return View();

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> DeffTypeIndex(int? pageNumber)
        {
            int pageSize = 100;
          

            var items = _context.DeffTypes.AsQueryable(); // Assuming DbSet is named "DeffTypes"
          //  var paginatedItems = await PaginatedList<DeffType>.CreateAsync(items, pageNumber, pageSize);
            return View(await PaginatedList<DeffType>.CreateAsync(items.AsNoTracking(), pageNumber ?? 1, pageSize));
          //  return View(paginatedItems);
        }

      

    }
}
