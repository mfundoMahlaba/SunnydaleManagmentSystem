using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public int? SubjectId { get; set; }
        public virtual Subject Subject { get; set; }
        public int? PeriodId { get; set; }
        public virtual Period Period { get; set; }
        public int MarkedByTeacherId { get; set; }
        public virtual Teacher MarkedByTeacher { get; set; }
        public DateTime MarkedAt { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public int GradeId { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        public string Remarks { get; set; } // Optional: e.g. "Late", "Excused"
    }

    public class Period
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // e.g. "Period 1", "Period 2"
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }


}