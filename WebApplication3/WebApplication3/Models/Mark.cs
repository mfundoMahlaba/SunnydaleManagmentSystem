using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Mark
    {
        [Key]
        public int MarkId { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public int GradeId { get; set; }
        public virtual Grade Grade { get; set; }

        public int TeacherId { get; set; } // for traceability
        public double Score { get; set; }
        public string Term { get; set; } // e.g., "Term 1"
        public string AssessmentType { get; set; } // e.g., "Test", "Exam", "Assignment"

    }
    


}