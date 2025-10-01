using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class AssignSubjectsViewModel
    {
        public int SelectedTeacherId { get; set; }

        public List<int> SelectedSubjectIds { get; set; }
        public List<int> SelectedGradeIds { get; set; }

        public IEnumerable<SelectListItem> Teachers { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }
        public IEnumerable<SelectListItem> Grades { get; set; }
        public string FilterTeacherName { get; set; }
        public int? FilterGradeId { get; set; }
    }
    public class TeacherAssignmentOverviewViewModel
    {
        public int AssignmentId { get; set; }

        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string GradeName { get; set; }

        // Optional extras
        public DateTime AssignedOn { get; set; }
        public string StreamName { get; set; } // if using streams
    }


}