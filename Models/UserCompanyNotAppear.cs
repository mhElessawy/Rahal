using System;
using System.Collections.Generic;

namespace RahalWeb.Models;

public partial class UserCompanyNotAppear
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? CompanyId { get; set; }

    public virtual CompanyInfo? Company { get; set; }

    public virtual PasswordDatum? User { get; set; }
}
