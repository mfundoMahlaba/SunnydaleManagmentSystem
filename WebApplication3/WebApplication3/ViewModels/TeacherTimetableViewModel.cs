using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TeacherTimetableViewModel
    {
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public string ClassroomName { get; set; }
        public string DayOfWeek { get; set; }
        public string TimeSlot { get; set; }
    }

}