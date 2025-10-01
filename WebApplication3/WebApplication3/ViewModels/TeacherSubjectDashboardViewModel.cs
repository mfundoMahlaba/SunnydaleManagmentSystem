using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TeacherSubjectDashboardViewModel
    {
        public int AssignmentId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string GradeName { get; set; }

        public List<StudentMarkEntry> Students { get; set; }
    }

    public class StudentMarkEntry
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public double? ExistingMark { get; set; }
    }

}