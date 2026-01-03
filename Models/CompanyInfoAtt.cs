using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models;

public partial class CompanyInfoAtt
{
    public int Id { get; set; }
    public int CompId { get; set; }
    public required string TitleData { get; set; }

    // This will store the file path in the database
    public string? PathFileData { get; set; }

    // This property should NOT be mapped to the database
    [NotMapped] // This tells EF Core to ignore this property
    public IFormFile? pdfFile1 { get; set; }

    public virtual CompanyInfo? Comp { get; set; }
}
