using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using RahalWeb.Models.MyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class CarInfoesController : Controller
    {
        private readonly RahalWebContext _context;
        public CarInfoesController(RahalWebContext context)
        {
            _context = context;
        }
        // GET: CarInfoes
        public async Task<IActionResult> Index(int? CarCodeString, string? CarNoSearch, int? CarTypeId, int? companyId, int? pageNumber)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep();
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            // Get companies for dropdown
            ViewBag.Companies = new SelectList(
                await _context.CompanyInfos
                 .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                    .Where(c => c.DeleteFlag == 0)
                    .OrderBy(c => c.CompNameAr)
                    .ToListAsync(),
                "Id",
                "CompNameAr",
                companyId);
            // Get Car Type for dropdown
            ViewBag.CarTypeId = new SelectList(
                 await _context.Deffs
                    .Where(c => c.DeffType == 21)
                    .OrderBy(c => c.DeffName)
                    .ToListAsync(),
                "Id",
                "DeffName",
                CarTypeId);
            // Base query with includes
            var query = _context.CarInfos
                .FromSqlRaw($"SELECT * FROM CarInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})")
                .Include(c => c.CarShape)
                .Include(c => c.CarType)
                .Include(c => c.Company)
                .Include(C => C.CarKind)
                .Include(c => c.User)
                .Where(m => m.DeleteFlag == 0)
                .OrderBy(e => e.CarCode);

            // Apply filters
            if (CarCodeString.HasValue)
            {
                query = (IOrderedQueryable<CarInfo>)query.Where(e => e.CarCode == CarCodeString);
            }

            if (!string.IsNullOrEmpty(CarNoSearch))
            {
                query = (IOrderedQueryable<CarInfo>)query.Where(e => e.CarNo!.Contains(CarNoSearch));
            }

            if (CarTypeId.HasValue)
            {
                query = (IOrderedQueryable<CarInfo>)query.Where(e => e.CarTypeId == CarTypeId.Value);
            }

            if (companyId.HasValue)
            {
                query = (IOrderedQueryable<CarInfo>)query.Where(e => e.CompanyId == companyId.Value);
            }
            // Create the view model query
            var viewModelQuery = query
                .Select(car => new CarInfoWithCreditsViewModel
                {
                    Car = car,
                    PayedCredit = (decimal)(_context.ContractDetails
                        .Where(cd => cd.Contract!.CarId == car.Id && cd.Status == 3)
                        .Sum(cd => (decimal?)cd.CarCredit) ?? 0),
                    RemainingCredit =( car.NoOfCredit * car.CarCredit ) - ((decimal)(_context.ContractDetails
                        .Where(cd => cd.Contract!.CarId == car.Id && cd.Status == 3)
                        .Sum(cd => (decimal?)cd.CarCredit) ?? 0))
                });

            // Store current search values for the view
            ViewData["CarCodeFilter"] = CarCodeString;
            ViewData["CarNoFilter"] = CarNoSearch;
            ViewData["CarTypeFilter"] = CarTypeId;
            ViewData["CompanyFilter"] = companyId;

            // Pagination with the view model
            int pageSize = 50;
            return View(await PaginatedList<CarInfoWithCreditsViewModel>.CreateAsync(viewModelQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        // GET: CarInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carInfo = await _context.CarInfos
                .Include(c => c.CarShape)
                .Include(c => c.CarType)
                .Include(c => c.Company)
                .Include (C=>C.CarKind)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carInfo == null)
            {
                return NotFound();
            }

            return View(carInfo);
        }
        // GET: CarInfoes/Create
        [HttpGet]
        public JsonResult GetKind(int Id)
        {

            var branches = _context.Deffs
                                  .Where(b => b.DeffParent  == Id)
                                  .Select(b => new { Id = b.Id, KindName = b.DeffName })
                                  .ToList();
            return Json(branches);
        }
        public IActionResult Create()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            ViewData["CarShapeId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 24), "Id", "DeffName");
            ViewData["CarTypeId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 21), "Id", "DeffName");
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");
            // ViewData["CarKindId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 24), "Id", "DeffName");
            ViewData["CarKindId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            return View();
        }
        // POST: CarInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,CarCode,RegDate,CarTypeId,CarKindId,CarShapeId,CarModel,CarNoOfSystemRound,CarShase,CarNo,CarColor,CarEndLicense,DeleteFlag,CarReg,CarEndDate,UserId")] CarInfo carInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            ViewData["CarShapeId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarShapeId);
            ViewData["CarTypeId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarTypeId);
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr", carInfo.CompanyId);
            ViewData["CarKindId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarKindId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", carInfo.UserId);
            return View(carInfo);
        }
        // GET: CarInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var carInfo = await _context.CarInfos.FindAsync(id);
            if (carInfo == null)
            {
                return NotFound();
            }
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values
            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");
            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);
            ViewData["CarShapeId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 24), "Id", "DeffName", carInfo.CarShapeId);
            ViewData["CarTypeId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 21), "Id", "DeffName", carInfo.CarTypeId);
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr", carInfo.CompanyId);
            ViewData["CarKindId"] = new SelectList(_context.Deffs.Where(a=>a.DeffParent== carInfo.CarTypeId), "Id", "DeffName", carInfo.CarKindId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", carInfo.UserId);
            return View(carInfo);
        }
        // POST: CarInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarInfo carInfo)
        {
            if (id != carInfo.Id)
            {
                return NotFound();
            }
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            if (ModelState.IsValid)
            {
                try
                {
                    carInfo.UserId = (int)ViewData["UserId"];
                    _context.Update(carInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarInfoExists(carInfo.Id))
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
            ViewData["CarShapeId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarShapeId);
            ViewData["CarTypeId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarTypeId);
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr", carInfo.CompanyId);
            ViewData["CarKindId"] = new SelectList(_context.Deffs, "Id", "DeffName", carInfo.CarKindId);
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName", carInfo.UserId);
            return View(carInfo);
        }
        // GET: CarInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var carInfo = await _context.CarInfos
                .Include(c => c.CarShape)
                .Include(c => c.CarType)
                .Include(c => c.Company)
                .Include(c=>c.CarKind )
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carInfo == null)
            {
                return NotFound();
            }

            return View(carInfo);
        }

        // POST: CarInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carInfo = await _context.CarInfos.FindAsync(id);
            if (carInfo != null)
            {
                _context.CarInfos.Remove(carInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool CarInfoExists(int id)
        {
            return _context.CarInfos.Any(e => e.Id == id);
        }
        #region Attatch File


        public IActionResult IndexAtt(int? id)
        {
            ViewBag.CarId = id;
            ViewBag.CarAtts = _context.CarInfoAtts.Where(a => a.Car!.Id == id).ToList();
            ViewBag.CarData = _context.CarInfos.FirstOrDefault(a => a.Id == id);

            return View();
        }

        [HttpGet]
        public IActionResult CreateAtt(int? CarId)
        {
            TempData.Keep();
            ViewBag.CarId = CarId;
            ViewBag.CarAtts = _context.CarInfoAtts.Where(a => a.Car!.Id == CarId).ToList();
            ViewBag.CarData = _context.CarInfos.FirstOrDefault(a => a.Id == CarId);
            
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAtt(CarInfoAtt model)
        {
            if (ModelState.IsValid)
            {
                if (model.pdfFile1 != null && model.pdfFile1.Length > 0)
                {
                    try
                    {
                        // Validate file type (even though client-side validation exists)
                        var allowedExtensions = new[] { ".pdf" };
                        var fileExtension = Path.GetExtension(model.pdfFile1.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("pdfFile1", "Only PDF files are allowed.");
                            return View(model);
                        }

                        // Set maximum file size (5MB in this example)
                        var maxFileSize = 5 * 1024 * 1024; // 5MB
                        if (model.pdfFile1.Length > maxFileSize)
                        {
                            ModelState.AddModelError("pdfFile1", "File size cannot exceed 5MB.");
                            return View(model);
                        }

                        // Generate a safe file name
                        var fileName = $"{Guid.NewGuid()}{fileExtension}";
                        string uploadsFolder = Path.Combine("wwwroot", "Car");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Create directory if it doesn't exist
                        Directory.CreateDirectory(uploadsFolder);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.pdfFile1.CopyToAsync(fileStream);
                        }

                        // Store relative path in database
                        model.PathFileData = $"/Car/{fileName}";
                    }
                    catch (Exception)
                    {
                        // Log the error
                        // _logger.LogError(ex, "Error uploading file");
                        ModelState.AddModelError("", "An error occurred while uploading the file.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("pdfFile1", "Please select a PDF file to upload.");
                    return View(model);
                }

                // Save to database
                _context.CarInfoAtts.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(IndexAtt), new { id = model.CarId });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditAtt(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Or redirect to an error page
            }

            // Assuming you're using Entity Framework with a DbContext
            var attachment = _context.CarInfoAtts.FirstOrDefault(a => a.Id == id);

            if (attachment == null)
            {
                return NotFound(); // Or handle the case where the attachment doesn't exist
            }

            return View(attachment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAtt(EmployeeInfoAtt model, IFormFile pdfFile1)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload if a new file was provided
                    if (pdfFile1 != null && pdfFile1.Length > 0)
                    {
                        // Save the new file and update model.PathFileData
                    }

                    // Update the entity in database
                    _context.Update(model);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(IndexAtt));
                }
                catch (Exception)
                {
                    // Log error
                    ModelState.AddModelError("", "Unable to save changes");
                }
            }

            return View(model);
        }
        public IActionResult DeleteAtt(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Or redirect to an error page
            }

            // Assuming you're using Entity Framework with a DbContext
            var attachment = _context.CarInfoAtts.Include(c => c.Car).FirstOrDefault(a => a.Id == id);

            if (attachment == null)
            {
                return NotFound(); // Or handle the case where the attachment doesn't exist
            }

            return View(attachment);
        }

        // POST: EmployeeInfoes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAtt(int id)
        {
            var carInfoAtt = await _context.CarInfoAtts.FindAsync(id);
            if (carInfoAtt != null)
            {
                _context.CarInfoAtts.Remove(carInfoAtt);
            }

            await _context.SaveChangesAsync();
            //   return RedirectToAction(nameof(IndexAtt));
            return RedirectToAction(nameof(IndexAtt), new { id = carInfoAtt!.CarId });
        }
        public IActionResult DownLoadAttatchment(int? id)
        {

            var fileData = _context.CarInfoAtts.FirstOrDefault(f => f.Id == id);
            if (fileData == null || string.IsNullOrEmpty(fileData.PathFileData))
            {
                return NotFound();
            }

            // Get absolute path (if stored as relative)
            string filePath = Path.Combine("wwwroot", fileData.PathFileData);
            filePath = "wwwroot" + filePath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            string fileName = Path.GetFileName(filePath);
            string contentType = GetMimeType(filePath);

            return File(fileData.PathFileData.Replace("wwwroot/", ""),
                        GetMimeType(fileData.PathFileData),
                        Path.GetFileName(fileData.PathFileData));
        }
        private string GetMimeType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(filePath, out contentType))
            {
                contentType = "application/octet-stream"; // Default if unknown
            }

            return contentType;
        }

        #endregion
    }
}
