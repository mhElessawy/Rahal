using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RahalWeb.Models;

public partial class CarInfoAtt
{
    public int Id { get; set; }

    public int? CarId { get; set; }

    public string? TitleData { get; set; }

    public string? PathFileData { get; set; }
    [NotMapped] // This tells EF Core to ignore this property
    public IFormFile? pdfFile1 { get; set; }
    public virtual CarInfo? Car { get; set; }
}
