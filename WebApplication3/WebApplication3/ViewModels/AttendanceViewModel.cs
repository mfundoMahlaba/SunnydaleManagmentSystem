using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class AttendanceEntryViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
        public string Remarks { get; set; }
    }

    public class AttendanceRegisterViewModel
    {
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public int? SubjectId { get; set; }

        public int? PeriodId { get; set; }

        public List<AttendanceEntryViewModel> Entries { get; set; }

        public IEnumerable<SelectListItem> GradeOptions { get; set; }
    }

    public class AttendanceHistoryViewModel
    {
        public DateTime Date { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
        public string Remarks { get; set; }
        public string SubjectName { get; set; }
        public string PeriodName { get; set; }
        public string MarkedBy { get; set; }
    }
    public class AttendanceHistoryFilterViewModel
    {
        public int? GradeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SubjectId { get; set; }

        public List<AttendanceHistoryViewModel> Records { get; set; }
        public IEnumerable<SelectListItem> GradeOptions { get; set; }
        public IEnumerable<SelectListItem> SubjectOptions { get; set; }
    }

    public class StudentAttendanceViewModel
    {
        public DateTime Date { get; set; }
        public string SubjectName { get; set; }
        public string PeriodName { get; set; }
        public bool IsPresent { get; set; }
        public string Remarks { get; set; }
    }

    public class StudentAttendanceFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SubjectId { get; set; }

        public List<StudentAttendanceViewModel> Records { get; set; }
        public IEnumerable<SelectListItem> SubjectOptions { get; set; }
    }







}