using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class DeffType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Deff> Deffs { get; set; } = new List<Deff>();
}
