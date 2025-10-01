using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class TimetableEntryViewModel
    {
        public int Id { get; set; } // For edit/delete actions
        public string ClassroomName { get; set; } // e.g. "10A"
        public string SubjectName { get; set; }
        public string TeacherName { get; set; }
        public string DayOfWeek { get; set; }
        public string TimeSlot { get; set; }
    }


    public class CreateTimetableViewModel
    {
        public int ClassroomId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public string DayOfWeek { get; set; }
        public string TimeSlot { get; set; }

        public List<SelectListItem> ClassroomOptions { get; set; }
        public List<SelectListItem> SubjectOptions { get; set; }
        public List<SelectListItem> TeacherOptions { get; set; }
    }






}