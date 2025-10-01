using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class StudentReportCardViewModel
    {
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public string Term { get; set; }

        public double? TestMark { get; set; }
        public double? ExamMark { get; set; }
        public double? AssignmentMark { get; set; }

        public double Average
        {
            get
            {
                var marks = new List<double>();
                if (TestMark.HasValue) marks.Add(TestMark.Value);
                if (ExamMark.HasValue) marks.Add(ExamMark.Value);
                if (AssignmentMark.HasValue) marks.Add(AssignmentMark.Value);
                return marks.Any() ? marks.Average() : 0;
            }
        }
    }

}