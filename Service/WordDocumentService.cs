using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using RahalWeb.Models;

public class WordDocumentService
{
    private readonly RahalWebContext _context;
    private readonly string _templatesRoot;

    public WordDocumentService(RahalWebContext context, string webRootPath)
    {
        _context = context;
        _templatesRoot = Path.Combine(webRootPath, "Templates");
    }

    public byte[] GeneratePermitDocument(int employeeId)
    {
        return GenerateDocument(employeeId, "NewPerm.docx");
    }

    public byte[] GenerateRenewalDocument(int employeeId)
    {
        return GenerateDocument(employeeId, "ReNewPermSp.docx");
    }

    public byte[] GenerateContractDocument(int contractId)
    {
        var contract = _context.Contracts
            .Include(c => c.Employee).ThenInclude(e => e!.Nationality)
            .Include(c => c.Employee).ThenInclude(e => e!.JobTitle)
            .Include(c => c.Employee).ThenInclude(e => e!.Company).ThenInclude(comp => comp!.CompActivate)
            .Include(c => c.Employee).ThenInclude(e => e!.Company).ThenInclude(comp => comp!.Location)
            .FirstOrDefault(c => c.Id == contractId);

        if (contract == null)
            throw new Exception("Contract not found");

        string templatePath = Path.Combine(_templatesRoot, "ContractNewEn.docx");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found", templatePath);

        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");
        File.Copy(templatePath, tempFilePath, true);

        try
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(tempFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                var bookmarkStarts = body.Descendants<BookmarkStart>().ToList();

                foreach (var bookmarkStart in bookmarkStarts)
                {
                    string value = GetContractBookmarkValue(bookmarkStart.Name, contract);
                    if (!string.IsNullOrEmpty(value))
                        ReplaceBookmarkText(bookmarkStart, value);
                }

                doc.Save();
            }

            return File.ReadAllBytes(tempFilePath);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    private static readonly string[] ArabicDayNames =
    {
        "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"
    };

    private string GetContractBookmarkValue(string bookmarkName, Contract contract)
    {
        var employee = contract.Employee;
        var company = employee?.Company;

        switch (bookmarkName)
        {
            case "CompNameAr":
            case "CompNameAr1":
                return company?.CompNameAr ?? "";
            case "CompNameEng":
            case "CompNameEng1":
                return company?.CompNameEn ?? "";

            case "CompOwnerAr":
            case "CompOwnerAr1":
            case "CompOwnerEng":
            case "CompOwnerEng1":
                return company?.OwnerName1 ?? "";
            case "CompOwnerCivilIDAr":
            case "CompOwnerCivilIDEng":
                return company?.OwnerCivilId1 ?? "";

            case "CompFileNoAr":
            case "CompFileNoEng":
                return company?.CompFileNo ?? "";

            case "CompActivateAr":
                return company?.CompActivate?.DeffName ?? "";
            case "CompActivateEng":
                return company?.CompActivate?.DeffNameEng ?? "";

            case "CompPlaceAr":
            case "CompPlaceEng":
                return company?.Address ?? "";

            case "ContractDateAr":
            case "ContractDateAr1":
            case "ContractDateEng1":
                return contract.ContractDate?.ToString("dd/MM/yyyy") ?? "";

            case "ContractDayAr":
                return contract.ContractDate.HasValue
                    ? ArabicDayNames[(int)contract.ContractDate.Value.DayOfWeek]
                    : "";

            case "ContractStartDateAr":
            case "ContractStartDateEng":
                return contract.StartDate?.ToString("dd/MM/yyyy") ?? "";

            case "ContractPeriodAr":
                return contract.NoOfDays.HasValue ? $"{contract.NoOfDays} يوم" : "";
            case "ContractPeriodEng":
                return contract.NoOfDays?.ToString() ?? "";

            case "EmpNameAr":
            case "EmpNameAr1":
                return employee?.FullNameAr ?? "";
            case "EmpNameEng":
            case "EmpNameEng1":
                return employee?.FullNameEn ?? "";

            case "EmpCivilIDAr":
            case "EmpCivilIDEng":
                return employee?.CivilId ?? "";

            case "EmpJobTitleAr":
            case "EmpJobTitleAr1":
                return employee?.JobTitle?.DeffName ?? "";
            case "EmpJobTitleEng":
            case "EmpJobTitleEng1":
                return employee?.JobTitle?.DeffNameEng ?? "";

            case "EmpNationalityAr":
                return employee?.Nationality?.DeffName ?? "";
            case "EmpNationalityEng":
                return employee?.Nationality?.DeffNameEng ?? "";

            case "EmpResidenceAr":
            case "EmpResidenceEng":
                return employee?.ResNo ?? "";

            case "EmpSalaryAr":
            case "EmpSalaryEng":
                return employee?.Salary?.ToString("N3") ?? "";
            case "EmpSalarTafketAr":
                return employee?.Salary.HasValue == true ? AmountToArabicWords(employee.Salary.Value) : "";

            default:
                return null;
        }
    }

    private static readonly string[] ArabicOnes =
    {
        "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"
    };
    private static readonly string[] ArabicTeens =
    {
        "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
    };
    private static readonly string[] ArabicTens =
    {
        "", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
    };
    private static readonly string[] ArabicHundreds =
    {
        "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة"
    };

    private static string ConvertGroupToArabicWords(int number)
    {
        var parts = new List<string>();

        int hundreds = number / 100;
        int remainder = number % 100;

        if (hundreds > 0)
            parts.Add(ArabicHundreds[hundreds]);

        if (remainder > 0)
        {
            if (remainder < 10)
                parts.Add(ArabicOnes[remainder]);
            else if (remainder < 20)
                parts.Add(ArabicTeens[remainder - 10]);
            else
            {
                int onesDigit = remainder % 10;
                int tensDigit = remainder / 10;
                parts.Add(onesDigit > 0
                    ? $"{ArabicOnes[onesDigit]} و{ArabicTens[tensDigit]}"
                    : ArabicTens[tensDigit]);
            }
        }

        return string.Join(" و", parts);
    }

    private static string ConvertToArabicWords(long number)
    {
        if (number == 0)
            return "صفر";

        var millions = (int)(number / 1_000_000);
        var thousands = (int)(number / 1000 % 1000);
        var rest = (int)(number % 1000);

        var parts = new List<string>();

        if (millions > 0)
        {
            parts.Add(millions == 1 ? "مليون"
                : millions == 2 ? "مليونان"
                : millions <= 10 ? $"{ConvertGroupToArabicWords(millions)} ملايين"
                : $"{ConvertGroupToArabicWords(millions)} مليون");
        }

        if (thousands > 0)
        {
            parts.Add(thousands == 1 ? "ألف"
                : thousands == 2 ? "ألفان"
                : thousands <= 10 ? $"{ConvertGroupToArabicWords(thousands)} آلاف"
                : $"{ConvertGroupToArabicWords(thousands)} ألف");
        }

        if (rest > 0)
            parts.Add(ConvertGroupToArabicWords(rest));

        return string.Join(" و", parts);
    }

    private static string AmountToArabicWords(decimal amount)
    {
        long dinars = (long)Math.Truncate(amount);
        int fils = (int)Math.Round((amount - dinars) * 1000, MidpointRounding.AwayFromZero);

        var result = $"فقط {ConvertToArabicWords(dinars)} دينار كويتي";
        if (fils > 0)
            result += $" و{ConvertToArabicWords(fils)} فلس";
        result += " لا غير";

        return result;
    }

    private byte[] GenerateDocument(int employeeId, string templateFileName)
    {
        // Get employee with related data
        var employee = _context.EmployeeInfos
            .Include(e => e.Nationality)
            .Include(e => e.JobTitle)
            .Include(e => e.Company)
            .FirstOrDefault(e => e.Id == employeeId);

        if (employee == null)
            throw new Exception("Employee not found");

        // Path to template
        string templatePath = Path.Combine(_templatesRoot, templateFileName);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found", templatePath);

        // Create temporary file with correct extension
        string extension = Path.GetExtension(templateFileName);
        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
        File.Copy(templatePath, tempFilePath, true);

        try
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(tempFilePath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Update bookmarks
                UpdateBookmarks(doc, employee);

                // Also update any text placeholders
                UpdateTextPlaceholders(body, employee);

                doc.Save();
            }

            return File.ReadAllBytes(tempFilePath);
        }
        finally
        {
            // Clean up
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    private void UpdateBookmarks(WordprocessingDocument doc, EmployeeInfo employee)
    {
        var body = doc.MainDocumentPart.Document.Body;

        // Get all bookmarks
        var bookmarkStarts = body.Descendants<BookmarkStart>().ToList();

        foreach (var bookmarkStart in bookmarkStarts)
        {
            string bookmarkName = bookmarkStart.Name;
            string value = GetBookmarkValue(bookmarkName, employee);

            if (!string.IsNullOrEmpty(value))
            {
                // Find the text to replace
                ReplaceBookmarkText(bookmarkStart, value);
            }
        }
    }

    private void ReplaceBookmarkText(BookmarkStart bookmarkStart, string newValue)
    {
        // Find bookmark end
        var bookmarkEnd = FindBookmarkEnd(bookmarkStart);

        if (bookmarkEnd == null)
            return;

        // Look for text between bookmark start and end
        var current = bookmarkStart.NextSibling();
        while (current != null && current != bookmarkEnd)
        {
            if (current is Run run)
            {
                var textElement = run.GetFirstChild<Text>();
                if (textElement != null)
                {
                    textElement.Text = newValue;
                    return;
                }
            }
            current = current.NextSibling();
        }

        // If no text element found, insert one
        if (bookmarkStart.Parent != null)
        {
            var newRun = new Run(new Text(newValue));
            bookmarkStart.Parent.InsertAfter(newRun, bookmarkStart);
        }
    }

    private BookmarkEnd FindBookmarkEnd(BookmarkStart bookmarkStart)
    {
        var body = bookmarkStart.Ancestors<Body>().FirstOrDefault();
        if (body == null)
            return null;

        var bookmarkEnds = body.Descendants<BookmarkEnd>().ToList();
        return bookmarkEnds.FirstOrDefault(be => be.Id == bookmarkStart.Id);
    }

    private string GetBookmarkValue(string bookmarkName, EmployeeInfo employee)
    {
        // Map bookmark names to employee properties
        switch (bookmarkName)
        {
            case "ResNo3":
            case "ResNo2":
            case "ResNo4":
            case "ResNo5":
            case "CivilId":
            case "LicenseNo":
            case "LicenseNo2":
                return employee.CivilId ?? "";
            case "FirstName":
            case "FirstName2":
                return employee.FirstNameAr!;
            case "SecondName2":
            case "SecondName":
                return employee.SecondNameAr!;
            case "ThirdName":
            case "ThirdName2":
                return employee.ThirdNameAr!;
            case "FourthName":
            case "FourthName2":
                return employee.ForthNameAr!;
            case "LastName":
            case "LastName2":
                return employee.LastNameAr!;
            case "ArName":
            case "FullArName3":
                return employee.FullNameAr!;
            case "NameEn":
            case "Name":
                return employee.FullNameEn ?? "";
            case "Nationality":
            case "Nationality2":
            case "Nationality3":
            case "Nationality5":
                return employee.Nationality?.DeffName ?? "";

            case "Gender":
            case "الجنس":
                return employee.Gender == 1 ? "ذكر" : "أنثى";

            case "JobTitle":
            case "JobTitle2":
            case "JobTitle3":
                return employee.JobTitle?.DeffName ?? "";

            case "BirthDate":
            case "تاريخ_الميلاد":
            case "تاريخ الميلاد":
                return employee.EmpBirthDate?.ToString("dd/MM/yyyy") ?? "";

            case "Address":
            case "عنوان":
            case "عنوان السكن":
                return employee.EmpAddress ?? "";
            case "WorkAddress":
            case "عنوان العمل":
                return employee.Company?.CompNameAr ?? "";

            case "EmpCode":
            case "كود":
                return employee.EmpCode?.ToString() ?? "";



            case "LicenseType":
            case "نوع_الرخصة":
            case "نوع الرخصة":
                return "خصوصي";

            case "LicenseNationality":
            case "جنسية_الرخصة":
            case "جنسيتها":
                return "الكويت";

            case "EmpStartLicence":
            case "تاريخ_الإصدار":
            case "تاريخ الاصدار":
                return employee.StartLicense?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");

            case "ExpiryDate":
            case "EmpEndLicence":
            case "تاريخ الانتهاء":
                return employee.EndLicense?.ToString("dd/MM/yyyy") ?? DateTime.Now.AddYears(1).ToString("dd/MM/yyyy");

            case "FileNumber":
            case "رقم_الملف":
            case "رقم الملف":
                return "7890";

            case "CurrentDate":
            case "التاريخ":
                return DateTime.Now.ToString("dd/MM/yyyy");

            case "RequestDate":
            case "تاريخ_الطلب":
            case "تاريخ الطلب":
                return DateTime.Now.ToString("dd/MM/yyyy");

            case "RequestType":
            case "نوع_الطلب":
            case "نوع الطلب":
                return "تجديد تصريح إجرة جوالة";

            case "Fees":
            case "الرسوم":
                return "د. كويتي";

            case "Phone":
            case "هاتف":
            case "TelNo":
                return employee.TelNo ?? "";

            case "Mobile":
            case "جوال":
            case "MobiileNo":
                return employee.MobiileNo ?? "";

            case "PassportNo":
            case "رقم_الجواز":
                return employee.PassportNo ?? "";

            case "BloodType":
            case "فصيلة_الدم":
            case "فصيلة الدم":
                return "O+"; // Default or add to employee model
            case "CompanyName":
                return employee.Company!.OwnerName1 ?? "";
            case "TraficLocationName":
                return "";
            default:
                return null;
        }
    }

    private void UpdateTextPlaceholders(Body body, EmployeeInfo employee)
    {
        var texts = body.Descendants<Text>().ToList();

        foreach (var text in texts)
        {
            string originalText = text.Text;

            // Replace common placeholders
            if (originalText.Contains("[CivilId]") || originalText.Contains("{{CivilId}}"))
                text.Text = originalText.Replace("[CivilId]", employee.CivilId ?? "")
                                       .Replace("{{CivilId}}", employee.CivilId ?? "");

            if (originalText.Contains("[NameAr]") || originalText.Contains("{{NameAr}}"))
                text.Text = originalText.Replace("[NameAr]", employee.FullNameAr ?? "")
                                       .Replace("{{NameAr}}", employee.FullNameAr ?? "");

            if (originalText.Contains("[NameEn]") || originalText.Contains("{{NameEn}}"))
                text.Text = originalText.Replace("[NameEn]", employee.FullNameEn ?? "")
                                       .Replace("{{NameEn}}", employee.FullNameEn ?? "");

            if (originalText.Contains("[Nationality]") || originalText.Contains("{{Nationality}}"))
                text.Text = originalText.Replace("[Nationality]", employee.Nationality?.DeffName ?? "")
                                       .Replace("{{Nationality}}", employee.Nationality?.DeffName ?? "");

            // Replace placeholder lines
            if (originalText.Contains("---") || originalText.Contains("...") || originalText.Contains("______"))
                text.Text = "";

            // Replace date placeholders
            if (originalText.Contains("[Date]") || originalText.Contains("{{Date}}"))
                text.Text = originalText.Replace("[Date]", DateTime.Now.ToString("dd/MM/yyyy"))
                                       .Replace("{{Date}}", DateTime.Now.ToString("dd/MM/yyyy"));
        }
    }

    // Method to list all bookmarks in template (for debugging)
    public List<string> GetTemplateBookmarks()
    {
        string templatePath = Path.Combine(_templatesRoot, "NewPerm.docx");

        if (!File.Exists(templatePath))
            return new List<string> { "Template file not found" };

        var bookmarks = new List<string>();

        using (WordprocessingDocument doc = WordprocessingDocument.Open(templatePath, false))
        {
            var body = doc.MainDocumentPart.Document.Body;
            var bookmarkStarts = body.Descendants<BookmarkStart>().ToList();

            foreach (var bookmark in bookmarkStarts)
            {
                bookmarks.Add(bookmark.Name);
            }
        }

        return bookmarks;
    }
}
