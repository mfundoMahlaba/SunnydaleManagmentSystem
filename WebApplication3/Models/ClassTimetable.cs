using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class ClassTimetable
    {
        [Key]
        public int Id { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public string DayOfWeek { get; set; } 
        public string TimeSlot { get; set; }  
    }

}