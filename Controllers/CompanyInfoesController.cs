using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RahalWeb.Controllers
{
    public class CompanyInfoesController : Controller
    {
        private readonly RahalWebContext _context;

        public CompanyInfoesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: CompanyInfoes
        public async Task<IActionResult> Index()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            var rahalWebContext = _context.CompanyInfos
                 .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                .Include(c => c.City).Include(c => c.CompActivate).Include(c => c.Location);
            return View(await rahalWebContext.ToListAsync());
        }

        // GET: CompanyInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos
                .Include(c => c.City)
                .Include(c => c.CompActivate)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInfo == null)
            {
                return NotFound();
            }

            return View(companyInfo);
        }

        // GET: CompanyInfoes/Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Deffs.Where(a=>a.DeffType == 4), "Id", "DeffName");
            ViewData["CompActivateId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 30), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            return View();
        }
        private async System.Threading.Tasks.Task ProcessImageAsync(IFormFile file, int latestId, int index)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UploadCompLogo");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var imageService = new ImageService();
            string filePath = Path.Combine(uploadsFolder, $"{latestId + 1}-{index}-{file.FileName}");
            await imageService.ResizeAndSaveImage(file, filePath, 300, 300);
        }

        // POST: CompanyInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CompanyInfo model)
        {
            if (ModelState.IsValid)
            {

                var checkCode = _context.CompanyInfos.Where(a => a.CompCode == model.CompCode);
                if (checkCode != null)
                {
                    ModelState.AddModelError("CompCode", "كود الشركه موجود مسبقا الرجاء إدخال كد آخر ");
                    return View(model);
                }


                var latestId = await _context.CompanyInfos.MaxAsync(d => (int?)d.Id) ?? 0;
                var tasks = new List<System.Threading.Tasks.Task>();
                if (model.ImageFile1 != null)
                {
                    tasks.Add(ProcessImageAsync(model.ImageFile1, latestId, 1));
                    model.CompLogo = (latestId + 1) + "-1-" + model.ImageFile1.FileName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            }
            ViewData["CityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 4), "Id", "DeffName");
            ViewData["CompActivateId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 30), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            return View(model);
        }

        // GET: CompanyInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos.FindAsync(id);
            if (companyInfo == null)
            {
                return NotFound();
            }
            ViewData["CityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 4), "Id", "DeffName");
            ViewData["CompActivateId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 30), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            return View(companyInfo);
        }

        // POST: CompanyInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  CompanyInfo model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var checkCode = await _context.CompanyInfos.AnyAsync(a => a.CompCode == model.CompCode && a.Id != model.Id);
                    if (checkCode)
                    {
                        ViewData["CityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 4), "Id", "DeffName");
                        ViewData["CompActivateId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 30), "Id", "DeffName");
                        ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");

                        ModelState.AddModelError("CompCode", "كود الشركه موجود مسبقا الرجاء إدخال كد آخر ");
                        return View(model);
                    }

                    var latestId = model.Id ;
                    var tasks = new List<System.Threading.Tasks.Task>();
                    if (model.ImageFile1 != null)
                    {
                        tasks.Add(ProcessImageAsync(model.ImageFile1, latestId, 1));
                        model.CompLogo = (latestId + 1) + "-1-" + model.ImageFile1.FileName;
                    }




                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyInfoExists(model.Id))
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
            ViewData["CityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 4), "Id", "DeffName");
            ViewData["CompActivateId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 30), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            return View(model);
        }

        // GET: CompanyInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos
                .Include(c => c.City)
                .Include(c => c.CompActivate)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInfo == null)
            {
                return NotFound();
            }
            return View(companyInfo);
        }

        // POST: CompanyInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyInfo = await _context.CompanyInfos.FindAsync(id);
            if (companyInfo != null)
            {
                _context.CompanyInfos.Remove(companyInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyInfoExists(int id)
        {
            return _context.CompanyInfos.Any(e => e.Id == id);
        }
   
        public async Task<IActionResult> Attatchment(int? id)
        {

            TempData.Keep();
            var CompAtt = _context.CompanyInfoAtts.Include(c => c.Comp).Where(c=>c.CompId == id);
            if (CompAtt != null)
            {
                ViewBag.CompName = CompAtt.First().Comp!.CompNameAr;
                ViewBag.CompId = CompAtt.First().CompId;


            }
            return View(await CompAtt!.ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateAttatch(int? CompId)
        {
            TempData.Keep();
            ViewBag.CompId = CompId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAttatch(CompanyInfoAtt model)
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
                        string uploadsFolder = Path.Combine("wwwroot", "Comp");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Create directory if it doesn't exist
                        Directory.CreateDirectory(uploadsFolder);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.pdfFile1.CopyToAsync(fileStream);
                        }

                        // Store relative path in database
                        model.PathFileData = $"/Comp/{fileName}";
                    }
                    catch (Exception ex)
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
                _context.CompanyInfoAtts.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { CompId = model.CompId });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditAttatchment(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Or redirect to an error page
            }

            // Assuming you're using Entity Framework with a DbContext
            var attachment = _context.CompanyInfoAtts.FirstOrDefault(a => a.Id == id);

            if (attachment == null)
            {
                return NotFound(); // Or handle the case where the attachment doesn't exist
            }

            return View(attachment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAttatchment(CompanyInfoAtt model, IFormFile pdfFile1)
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

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log error
                    ModelState.AddModelError("", "Unable to save changes");
                }
            }

            return View(model);
        }


        public IActionResult DownLoadAttatchment(int? id)
        {

            var fileData = _context.CompanyInfoAtts.FirstOrDefault(f => f.Id == id);
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

        public IActionResult DeleteAttatchment(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Or redirect to an error page
            }

            // Assuming you're using Entity Framework with a DbContext
            var attachment = _context.CompanyInfoAtts.Include(c => c.Comp).FirstOrDefault(a => a.Id == id);

            if (attachment == null)
            {
                return NotFound(); // Or handle the case where the attachment doesn't exist
            }

            return View(attachment);
        }

        // POST: EmployeeInfoes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttatchment(int id)
        {
            var companyInfoAtt = await _context.CompanyInfoAtts.FindAsync(id);
            if (companyInfoAtt != null)
            {
                _context.CompanyInfoAtts.Remove(companyInfoAtt);
            }

            await _context.SaveChangesAsync();
            //   return RedirectToAction(nameof(IndexAtt));
            return RedirectToAction(nameof(Attatchment), new { id = companyInfoAtt!.CompId });
        }

    }
}
