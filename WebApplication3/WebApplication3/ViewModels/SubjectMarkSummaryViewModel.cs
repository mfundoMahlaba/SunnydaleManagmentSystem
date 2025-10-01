using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class SubjectMarkSummaryViewModel
    {
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public string Term { get; set; }
        public string AssessmentType { get; set; }
        public int TotalStudents { get; set; }
        public int MarksCaptured { get; set; }
        public double AverageMark { get; set; }
    }

}