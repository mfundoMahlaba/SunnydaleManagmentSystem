using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class SubjectEnrollmentViewModel
    {
        public int SelectedGradeId { get; set; }
        public int? SelectedStreamId { get; set; }
        public List<int> SelectedSubjectIds { get; set; }

        public IEnumerable<SelectListItem> Grades { get; set; }
        public IEnumerable<SelectListItem> Streams { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }

        // --- Added for previous results and statistics ---
        public List<PreviousResult> PreviousResults { get; set; }
        public double? AveragePercentage { get; set; }
        public int FailedCount { get; set; }
        public int PreviousGrade { get; set; }
        public int SelectedYear { get; set; }
        public IEnumerable<SelectListItem> Years { get; set; }

        public string SelectedStudentType { get; set; }
        public IEnumerable<SelectListItem> StudentTypeList { get; set; }
    }

    public class EnrolledGradeViewModel
    {
        public string GradeName { get; set; }
        public List<EnrolledSubjectViewModel> Subjects { get; set; }
    }
    public class EnrolledSubjectViewModel
    {
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
    }



}