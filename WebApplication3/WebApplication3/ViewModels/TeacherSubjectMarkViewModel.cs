using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TeacherSubjectMarkViewModel
    {
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public List<StudentMarkEntry01> Students { get; set; }
    }

    public class StudentMarkEntry01
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Term { get; set; }
        public string AssessmentType { get; set; }
        public double? Mark { get; set; }
    }

}