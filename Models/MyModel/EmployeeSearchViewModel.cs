using System.ComponentModel.DataAnnotations;

namespace RahalWeb.Models.MyModel
{
    public class EmployeeSearchViewModel
    {
        [Display(Name = "كود الموظف")]
        public int? EmpCode { get; set; }

        [Display(Name = "الرقم المدني")]
        public string? CivilId { get; set; }

        [Display(Name = "الاسم")]
        public string? Name { get; set; }

        public int? SelectedEmployeeId { get; set; }
        public List<EmployeeInfo>? Employees { get; set; }

    }
}
