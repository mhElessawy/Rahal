using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Data;
using RahalWeb.Models;
using RahalWeb.Models.MyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
namespace RahalWeb.Controllers
{
    public class EmployeeInfoesController : Controller
    {
        private readonly RahalWebContext _context;

        public EmployeeInfoesController(RahalWebContext context)
        {
            _context = context;
        }

        // GET: EmployeeInfoes
        public async Task<IActionResult> Index(int? searchString, string nameSearch, int? companyId, int? pageNumber)
        {
            try
            {
                TempData["Username"] = HttpContext.Session.GetString("Username");
                ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
                TempData.Keep(); // Keeps all TempData values

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

                // Base query with includes
                var query = _context.EmployeeInfos
                     .FromSqlRaw($"SELECT * FROM EmployeeInfo WHERE DeleteFlag = 0 AND CompanyId IN ({companyIdsString})")
                    .Include(e => e.Company)
                    .Where(a => a.DeleteFlag == 0)
                    .OrderBy(e => e.EmpCode);

                // Apply filters
                if (searchString.HasValue)
                {
                    query = (IOrderedQueryable<EmployeeInfo>)query.Where(e => e.EmpCode == searchString);
                }

                if (!string.IsNullOrEmpty(nameSearch))
                {
                    query = (IOrderedQueryable<EmployeeInfo>)query.Where(e =>
                        e.FullNameAr!.Contains(nameSearch) ||
                        e.FullNameEn!.Contains(nameSearch) ||
                        e.LastNameAr!.Contains(nameSearch) ||
                        e.LastNameEn!.Contains(nameSearch));
                }

                if (companyId.HasValue)
                {
                    query = (IOrderedQueryable<EmployeeInfo>)query.Where(e => e.CompanyId == companyId.Value);
                }

                // Store current search values for the view
                ViewData["CurrentFilter"] = searchString;
                ViewData["NameFilter"] = nameSearch;
                ViewData["CompanyFilter"] = companyId;

                // Pagination
                int pageSize = 50; // Set your page size
                return View(await PaginatedList<EmployeeInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
               
            }
            catch (Exception ex)
            {
                // Log the complete exception details
                Console.WriteLine(ex.ToString());
                throw; // Re-throw after logging
            }

           
        }

        // GET: EmployeeInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeInfo = await _context.EmployeeInfos
                .Include(e => e.Company)
                .Include(e => e.JobTitle)
                .Include(e => e.Nationality)
                .Include(e => e.Relation)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeInfo == null)
            {
                return NotFound();
            }

            return View(employeeInfo);
        }

        // GET: EmployeeInfoes/Create
        public IActionResult Create()
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                 .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                 , "Id", "CompNameAr");
            ViewData["JobTitleId"] = new SelectList(_context.Deffs.Where(a=>a.DeffType== 5), "Id", "DeffName");
            ViewData["NationalityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 2), "Id", "DeffName");
            ViewData["RelationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 7), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            return View();
        }

        // POST: EmployeeInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( EmployeeInfo employeeInfo)
        {
            TempData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            TempData.Keep(); // Keeps all TempData values

            TempData["UserCompanyData"] = HttpContext.Session.GetString("UserCompanyData");

            var userCompanyData = TempData["UserCompanyData"]?.ToString();
            var companyIds = userCompanyData.Split(',').Select(int.Parse).ToList();
            var companyIdsString = string.Join(",", companyIds);

            if (ModelState.IsValid)
            {


                bool empCodeExists = _context.EmployeeInfos.Any(a => a.EmpCode == employeeInfo.EmpCode && a.DeleteFlag == 0);

                if (empCodeExists)
                {
                    ModelState.AddModelError("Empcode", "كود الموظف موجود مسبقا");
                }
                else
                {
                    bool civilIdExists = _context.EmployeeInfos
                        .Any(a => a.CivilId == employeeInfo.CivilId && a.DeleteFlag == 0);

                    if (civilIdExists)
                    {
                        ModelState.AddModelError("CivilId", "الرقم المدني موجود مسبقا");
                    }
                    else
                    {
                        employeeInfo.FullNameAr = employeeInfo.FirstNameAr + " " + employeeInfo.SecondNameAr + " " + employeeInfo.ThirdNameAr + " " + employeeInfo.LastNameAr;
                        employeeInfo.FullNameEn = employeeInfo.FirstNameEn + " " + employeeInfo.SecondNameEn + " " + employeeInfo.ThirdNameEn + " " + employeeInfo.LastNameEn;
                        employeeInfo.DeleteFlag = 0;
                        employeeInfo.UserId = HttpContext.Session.GetInt32("UserId");

                        _context.Add(employeeInfo);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                 .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                 , "Id", "CompNameAr");
            ViewData["JobTitleId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 5), "Id", "DeffName");
            ViewData["NationalityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 2), "Id", "DeffName");
            ViewData["RelationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 7), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            return View(employeeInfo);
        }

        // GET: EmployeeInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
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


            var employeeInfo = await _context.EmployeeInfos.FindAsync(id);
            if (employeeInfo == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");
            ViewData["JobTitleId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 5), "Id", "DeffName");
            ViewData["NationalityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 2), "Id", "DeffName");
            ViewData["RelationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 7), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            return View(employeeInfo);
        }

        // POST: EmployeeInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeInfo employeeInfo)
        {
            if (id != employeeInfo.Id)
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
                    employeeInfo.FullNameAr = employeeInfo.FirstNameAr + " " + employeeInfo.SecondNameAr + " " + employeeInfo.ThirdNameAr + " " + employeeInfo.LastNameAr;
                    employeeInfo.FullNameEn = employeeInfo.FirstNameEn + " " + employeeInfo.SecondNameEn + " " + employeeInfo.ThirdNameEn + " " + employeeInfo.LastNameEn;
                    employeeInfo.DeleteFlag = 0;
                    employeeInfo.UserId = 1;
                    _context.Update(employeeInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeInfoExists(employeeInfo.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.CompanyInfos
                .FromSqlRaw($"SELECT * FROM CompanyInfo WHERE DeleteFlag = 0 AND Id IN ({companyIdsString})")
                , "Id", "CompNameAr");
            ViewData["JobTitleId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 5), "Id", "DeffName");
            ViewData["NationalityId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 2), "Id", "DeffName");
            ViewData["RelationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 7), "Id", "DeffName");
            ViewData["LocationId"] = new SelectList(_context.Deffs.Where(a => a.DeffType == 3), "Id", "DeffName");
            ViewData["UserId"] = new SelectList(_context.PasswordData, "Id", "UserFullName");
            return View(employeeInfo);
        }

        // GET: EmployeeInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeInfo = await _context.EmployeeInfos
                .Include(e => e.Company)
                .Include(e => e.JobTitle)
                .Include(e => e.Nationality)
                .Include(e => e.Relation)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeInfo == null)
            {
                return NotFound();
            }

            return View(employeeInfo);
        }

        // POST: EmployeeInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeInfo = await _context.EmployeeInfos.FindAsync(id);
            if (employeeInfo != null)
            {
                _context.EmployeeInfos.Remove(employeeInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeInfoExists(int id)
        {
            return _context.EmployeeInfos.Any(e => e.Id == id);
        }

        #region Attatch File


        public IActionResult IndexAtt(int? id)
        {
            ViewBag.EmployeeId = id;
            ViewBag.EmployeeAtts = _context.EmployeeInfoAtts.Where(a => a.Emp!.Id == id).ToList();
            ViewBag.EmployeeData = _context.EmployeeInfos.FirstOrDefault(a => a.Id == id);

            return View();
        }

        [HttpGet]
        public IActionResult CreateAtt(int? EmpId)
        {
            TempData.Keep();
            ViewBag.EmployeeId = EmpId;
            ViewBag.EmployeeAtts = _context.EmployeeInfoAtts.Where(a => a.Emp!.Id == EmpId).ToList();
            ViewBag.EmployeeData = _context.EmployeeInfos.FirstOrDefault(a => a.Id == EmpId);
            ViewBag.EmpId = EmpId;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAtt(EmployeeInfoAtt model)
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
                        string uploadsFolder = Path.Combine("wwwroot", "Emp");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Create directory if it doesn't exist
                        Directory.CreateDirectory(uploadsFolder);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.pdfFile1.CopyToAsync(fileStream);
                        }

                        // Store relative path in database
                        model.PathFileData = $"/Emp/{fileName}";
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
                _context.EmployeeInfoAtts.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(IndexAtt), new { id = model.EmpId });
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
            var attachment = _context.EmployeeInfoAtts.FirstOrDefault(a => a.Id == id);

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
            var attachment = _context.EmployeeInfoAtts.Include(c=>c.Emp).FirstOrDefault(a => a.Id == id);

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
            var employeeInfoAtt = await _context.EmployeeInfoAtts.FindAsync(id);
            if (employeeInfoAtt != null)
            {
                _context.EmployeeInfoAtts.Remove(employeeInfoAtt);
            }

            await _context.SaveChangesAsync();
         //   return RedirectToAction(nameof(IndexAtt));
            return RedirectToAction(nameof(IndexAtt), new { id = employeeInfoAtt!.EmpId });
        }
        public IActionResult DownLoadAttatchment(int? id)
        {

            var fileData = _context.EmployeeInfoAtts.FirstOrDefault(f => f.Id == id);
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
        [HttpGet]
        public async Task<IActionResult> RecievedMoney(int? UserId, DateOnly? FromDate, DateOnly? ToDate)
        {
            TempData.Keep();
            // Get companies for dropdown
            ViewBag.UserId = new SelectList(
                await _context.PasswordData
                    .Where(c => c.DeleteFlag == 0)
                    .OrderBy(c => c.UserFullName)
                    .ToListAsync(),
                "Id",
                "UserName",
                UserId);

            ViewBag.UserRecievedId = ViewBag.UserId;

            var query = _context.Bills
                .Include(e => e.Contract)
                .Include(e => e.User)
                .Include(e => e.Employee)
                .Where(a => a.DeleteFlag == 0 && a.Contract!.Status==0 && a.Contract.DeleteFlag == 0 && a.Employee!.DeleteFlag==0);

            if (UserId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(a => a.UserRecievedId == UserId.Value)
                    .OrderBy(e => e.Employee!.EmpCode);

                if (FromDate.HasValue && FromDate.Value != default)
                {
                    query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate >= FromDate);
                }
                if (ToDate.HasValue && ToDate.Value != default)
                {
                    query = (IOrderedQueryable<Bill>)query.Where(e => e.BillDate <= ToDate);
                }
            }
            else
            {
                query = (IOrderedQueryable<Bill>)query.Where(a => false);
            }

            var totalBillPayed = query.Sum(b => b.BillPayed) ?? 0;



            var queryDebit = _context.DebitPayInfos
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Include(v => v.ViolationInfo)
                .ThenInclude(vi => vi!.Employee)
                .Where(m => m.DeleteFlag == 0 && m.DebitInfo!.Emp!.DeleteFlag == 0 )
                .OrderBy(e => e.DebitPayNo);

            if (UserId.HasValue)
            {
                queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.UserRecievedId == UserId);
                if (FromDate.HasValue && FromDate.Value != default)
                {
                    queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.DebitPayDate >= FromDate);
                }
                if (ToDate.HasValue && ToDate.Value != default)
                {
                    queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.DebitPayDate <= ToDate);
                }
            }
            else 
            {
                queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(a => false);
            }

            var totalDebitPayed = queryDebit.Sum(item => item.DebitPayQty) ?? 0;




            var queryCompanyDebitDetails = _context.CompanyDebitDetails
                .Include(c => c.CompanyDebits)
                .Include(c => c.CompanyDebits!.Employee)
                .Include(c => c.UserInfo)
                .Where(m => m.CompDebitType == 1)
                .OrderBy(e => e.CompDebitDetailsNo);

            if (UserId.HasValue)
            {
                queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.UserRecievedId == UserId);
                if (FromDate.HasValue && FromDate.Value != default)
                {
                    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate >= FromDate);
                }
                if (ToDate.HasValue && ToDate.Value != default)
                {
                    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate <= ToDate);
                }
            }
            else
            {
                queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(a => false);
            }

            var totalCompanyDebitPayed = queryCompanyDebitDetails.Sum(item => item.CompDebitPayed) ?? 0;



            var totalCombined = totalBillPayed + totalDebitPayed + totalCompanyDebitPayed ;

            var viewModel = new ReceivedMoneyViewModel
            {
                DebitPayInfos = await queryDebit.ToListAsync(),
                Bills = await query.ToListAsync(),
                companyDebitDetails = await queryCompanyDebitDetails.ToListAsync(),
                TotalBillPayed = totalCombined
            };


            ViewData["UserIdFilter"] = UserId;
            ViewData["FromDateFilter"] = FromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDateFilter"] = ToDate?.ToString("yyyy-MM-dd");



            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RecievedMoney(int? UserId,DateOnly? FromDate,DateOnly? ToDate,int? UserRecievedId,string PasswordRecieved)
        {
            TempData.Keep();
            // Get companies for dropdown
            if (UserRecievedId == null || string.IsNullOrEmpty(PasswordRecieved))
            {
                TempData["ErrorMessage"] = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                return RedirectToAction("RecievedMoney", new { UserId, FromDate, ToDate });
            }

            var user = _context.PasswordData.FirstOrDefault(u => u.Id == UserRecievedId);

            if (user == null)
            {
                // Security best practice: don't reveal if user exists
                ModelState.AddModelError("", "اسم المستخدم غير موجود");
                return View();
            }
            // 2. Verify the password
            if (!VerifyPassword(user.Password!, PasswordRecieved))
            {
                ModelState.AddModelError("PasswordRecieved", "كلمة المرور غير صحيحة");
                return View();
            }

            // 3. Process the money receiving logic here
            try
            {
                // Get all bills that match the criteria and haven't been received yet

                var billsToUpdate = _context.Bills
                    .Include(e => e.Contract)
                    .Include(e => e.User)
                    .Include(e => e.Employee)
                    .Where(a => a.DeleteFlag == 0 && (a.Contract!.Status == 0 && a.Contract!.DeleteFlag == 0));
                if (UserId.HasValue)
                {
                    billsToUpdate = (IOrderedQueryable<Bill>)billsToUpdate.Where(a => a.UserRecievedId == UserId.Value);

                    if (FromDate.HasValue && FromDate.Value != default)
                    {
                        billsToUpdate = (IOrderedQueryable<Bill>)billsToUpdate.Where(e => e.BillDate >= FromDate);
                    }
                    if (ToDate.HasValue && ToDate.Value != default)
                    {
                        billsToUpdate = (IOrderedQueryable<Bill>)billsToUpdate.Where(e => e.BillDate <= ToDate);
                    }
                }
                else
                {
                    billsToUpdate = (IOrderedQueryable<Bill>)billsToUpdate.Where(a => false);
                }


                // Get all debit payments that match the criteria
                var debitsToUpdate = _context.DebitPayInfos
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Include(v => v.ViolationInfo)
                .ThenInclude(vi => vi!.Employee)
                .Where(m => m.DeleteFlag == 0)
                .OrderBy(e => e.DebitPayNo);


                if (UserId.HasValue)
                {
                    debitsToUpdate = (IOrderedQueryable<DebitPayInfo>)debitsToUpdate.Where(e => e.UserRecievedId == UserId);
                    if (FromDate.HasValue && FromDate.Value != default)
                    {
                        debitsToUpdate = (IOrderedQueryable<DebitPayInfo>)debitsToUpdate.Where(e => e.DebitPayDate >= FromDate);
                    }
                    if (ToDate.HasValue && ToDate.Value != default)
                    {
                        debitsToUpdate = (IOrderedQueryable<DebitPayInfo>)debitsToUpdate.Where(e => e.DebitPayDate <= ToDate);
                    }
                }
                else
                {
                    debitsToUpdate = (IOrderedQueryable<DebitPayInfo>)debitsToUpdate.Where(a => false);
                }



                var queryCompanyDebitDetails = _context.CompanyDebitDetails
               .Include(c => c.CompanyDebits)
               .Include(c => c.CompanyDebits!.Employee)
               .Include(c => c.UserInfo)
               .Where(m => m.CompDebitType == 1)
               .OrderBy(e => e.CompDebitDetailsNo);

                if (UserId.HasValue)
                {
                    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.UserRecievedId == UserId);
                    if (FromDate.HasValue && FromDate.Value != default)
                    {
                        queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate >= FromDate);
                    }
                    if (ToDate.HasValue && ToDate.Value != default)
                    {
                        queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate <= ToDate);
                    }
                }
                else
                {
                    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(a => false);
                }


               // int maxRecievedNoBill = _context.Bills.Max(a => Convert.ToInt32(a.UserRecievedNo));
                int maxRecievedNoBill = _context.Bills.Max(a => (int)a.UserRecievedNo!);
                maxRecievedNoBill = maxRecievedNoBill + 1;


                // Update bills
                foreach (var bill in billsToUpdate)
                {
                    bill.UserRecievedId = UserRecievedId;
                    bill.UserRecievedDate = DateOnly.FromDateTime(DateTime.Now);
                    bill.UserRecievedNo = maxRecievedNoBill;
                    _context.Update(bill);
                }


             //    int maxRecievedNoDebit = _context.DebitPayInfos.Max(a => Convert.ToInt32(a.UserRecievedNo));
                int maxRecievedNoDebit = _context.DebitPayInfos.Max(a => (int)a.UserRecievedNo!);
                maxRecievedNoDebit = maxRecievedNoDebit + 1;

                // Update debit payments
                foreach (var debit in debitsToUpdate)
                {
                    debit.UserRecievedId = UserRecievedId;
                    debit.UserRecievedDate = DateOnly.FromDateTime(DateTime.Now);
                    debit.UserRecievedNo = maxRecievedNoDebit;
                    _context.Update(debit);
                }


                //int maxRecievedNoCompDebit = _context.CompanyDebitDetails.Max(a => Convert.ToInt32(a.UserRecievedNo));
                int maxRecievedNoCompDebit = _context.CompanyDebitDetails.Max(a => (int)a.UserRecievedNo!);
               

                maxRecievedNoCompDebit = maxRecievedNoCompDebit + 1;

                foreach (var compDebit in queryCompanyDebitDetails)
                {
                    compDebit.UserRecievedId = UserRecievedId;
                    compDebit.UserRecievedDate = DateOnly.FromDateTime(DateTime.Now);
                    compDebit.UserRecievedNo = maxRecievedNoCompDebit;
                    _context.Update(compDebit);
                }



                // Save changes
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم استلام المبلغ بنجاح";
               //print Recieved data 
                return RedirectToAction("PrintRecievedMoney", new { UserRecievedId, FromDate = DateOnly.FromDateTime(DateTime.Now), ToDate = DateOnly.FromDateTime(DateTime.Now), maxRecievedNoBill, maxRecievedNoDebit, maxRecievedNoCompDebit });

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء استلام المبلغ: {ex.Message}";
            }

            return RedirectToAction("RecievedMoney", new { UserId, FromDate, ToDate });
        }
        public async Task<IActionResult> PrintRecievedMoney(int? UserRecievedId, DateOnly? FromDate,DateOnly? ToDate,int? maxRecievedNoBill , int? maxRecievedNoDebit,int? maxRecievedNoCompDebit)
        {
            TempData.Keep();
            // Get companies for dropdown
            var query = _context.Bills
                .Include(e => e.Contract)
                .Include(e => e.User)
                .Include(e => e.Employee)
                .Where(a => a.UserRecievedNo == maxRecievedNoBill);
            if (UserRecievedId.HasValue)
            {
                query = (IOrderedQueryable<Bill>)query.Where(a => a.UserRecievedId == UserRecievedId.Value)
                    .OrderBy(e => e.Employee!.EmpCode);

                if (FromDate.HasValue && FromDate.Value != default)
                {
                    query = (IOrderedQueryable<Bill>)query.Where(e => e.UserRecievedDate >= FromDate);
                }
                if (ToDate.HasValue && ToDate.Value != default)
                {
                    query = (IOrderedQueryable<Bill>)query.Where(e => e.UserRecievedDate <= ToDate);
                }
            }
            else
            {
                query = (IOrderedQueryable<Bill>)query.Where(a => false);
            }

            var totalBillPayed = query.Sum(b => b.BillPayed) ?? 0;

            var queryDebit = _context.DebitPayInfos
                .Include(c => c.DebitInfo)
                .Include(c => c.DebitInfo!.Emp)
                .Include(c => c.DebitInfo!.DebitType)
                .Include(c => c.User)
                .Include(c => c.UserRecieved)
                .Include(v => v.ViolationInfo)
                .ThenInclude(vi => vi!.Employee)
                .Where(m => m.UserRecievedNo == maxRecievedNoDebit)
                .OrderBy(e => e.DebitPayNo);

            if (UserRecievedId.HasValue)
            {
                queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.UserRecievedId == UserRecievedId.Value);
                if (FromDate.HasValue && FromDate.Value != default)
                {
                    queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.UserRecievedDate >= FromDate);
                }
                if (ToDate.HasValue && ToDate.Value != default)
                {
                    queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(e => e.UserRecievedDate <= ToDate);
                }
            }
            else
            {
                queryDebit = (IOrderedQueryable<DebitPayInfo>)queryDebit.Where(a => false);
            }



            var totalDebitPayed = queryDebit.Sum(item => item.DebitPayQty) ?? 0;



            var queryCompanyDebitDetails = _context.CompanyDebitDetails
                .Include(c => c.CompanyDebits)
                .Include(c => c.CompanyDebits!.Employee)
                .Include(c => c.UserInfo)
                .Include(c => c.UserInfoRecieve)
                .Where(m => m.CompDebitType == 1 && m.UserRecievedNo == maxRecievedNoCompDebit)
                .OrderBy(e => e.CompDebitDetailsNo);

            //if (UserRecievedId.HasValue)
            //{
            //    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.UserRecievedId == UserRecievedId);
            //    if (FromDate.HasValue && FromDate.Value != default)
            //    {
            //        queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate >= FromDate);
            //    }
            //    if (ToDate.HasValue && ToDate.Value != default)
            //    {
            //        queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(e => e.CompDebitDate <= ToDate);
            //    }
            //}
            //else
            //{
            //    queryCompanyDebitDetails = (IOrderedQueryable<CompanyDebitDetails>)queryCompanyDebitDetails.Where(a => false);
            //}

            var totalCompanyDebitPayed = queryCompanyDebitDetails.Sum(item => item.CompDebitPayed) ?? 0;

            var totalCombined = totalBillPayed + totalDebitPayed + totalCompanyDebitPayed;

            var viewModel = new ReceivedMoneyViewModel
            {
                DebitPayInfos = await queryDebit.ToListAsync(),
                Bills = await query.ToListAsync(),
                companyDebitDetails = await queryCompanyDebitDetails.ToListAsync(),
                TotalBillPayed = totalCombined
            };
            ViewBag.TotalBillPayed = totalCombined;
            return View(viewModel);
        }
        public bool VerifyPassword(string storedHash, string providedPassword)
        {
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(null!, storedHash, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        public IActionResult ExportToExcel(int? searchString = null, string? nameSearch = "", int? companyId = null)
        {
            try
            {
                // Get filtered data
                var employees = GetFilteredEmployees(searchString, nameSearch, companyId);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("الموظفين");

                    // Set RTL direction for the worksheet
                    worksheet.RightToLeft = true;

                    // Add headers
                    var headers = new string[]
                    {
                "كود الموظف", "اسم الموظف", "الرقم المدني", "رقم الهاتف", "اسم الشركة"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // Add data
                    int row = 2;
                    foreach (var employee in employees)
                    {
                        worksheet.Cell(row, 1).Value = employee.EmpCode;
                        worksheet.Cell(row, 2).Value = employee.FullNameAr;
                        worksheet.Cell(row, 3).Value = employee.CivilId;
                        worksheet.Cell(row, 4).Value = employee.MobiileNo;
                        worksheet.Cell(row, 5).Value = employee.Company?.CompNameAr;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Create memory stream
                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset stream position

                    string fileName = $"الموظفين_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    // This will trigger the browser's "Save As" dialog
                    return File(
                        fileStream: stream,
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: fileName);
                }
            }
            catch (Exception ex)
            {
                // Log error
               // _logger.LogError(ex, "Error exporting employees to Excel");
                return RedirectToAction("Index");
            }
        }

        // Helper method to get filtered data
        private IQueryable<EmployeeInfo> GetFilteredEmployees(int? searchString, string? nameSearch, int? companyId)
        {
            var query = _context.EmployeeInfos
                .Include(e => e.Company)
                .AsQueryable();

            if (searchString.HasValue)
            {
                query = query.Where(e => e.EmpCode == searchString);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(e => e.FullNameAr.Contains(nameSearch));
            }

            if (companyId.HasValue)
            {
                query = query.Where(e => e.CompanyId == companyId.Value);
            }

            return query;
        }
    }
}
