using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class EnrollmentViewModel
    {
        // --- Student & Enrollment Info ---
        public int Year { get; set; }
        public string StudentType { get; set; } // "New" or "Returning"
        public int SelectedGrade { get; set; }  // GradeLevel = 8–12
        public int? SelectedStreamId { get; set; } // Only for Grade 10–12

        // --- Dropdown Lists ---
        public List<SelectListItem> YearList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StudentTypeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> GradeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StreamList { get; set; } = new List<SelectListItem>();

        // --- Subject Selection ---
        public List<SubjectCheckbox> Subjects { get; set; } = new List<SubjectCheckbox>();
        public List<int> SelectedSubjectIds { get; set; } = new List<int>();

        // --- Previous Academic History ---
        public List<PreviousResult> PreviousResults { get; set; } = new List<PreviousResult>();

        // --- Validation Flag ---
        public bool CanSubmit { get; set; }
    }

    // ✅ Helper class for rendering subject checkboxes
    public class SubjectCheckbox
    {
        public int Id { get; set; }      // SubjectId
        public string Name { get; set; } // Subject Name
        public bool Selected { get; set; }
    }


}