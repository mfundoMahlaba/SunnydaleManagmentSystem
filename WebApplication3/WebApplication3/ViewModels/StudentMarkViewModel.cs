using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class StudentMarkViewModel
    {
        public string SubjectName { get; set; }
        public double Score { get; set; }
    }

    public class ReportCardViewModel
    {
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public List<StudentMarkViewModel> Marks { get; set; }
    }


}