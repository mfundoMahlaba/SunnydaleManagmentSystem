using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class EnrollmentSubject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnrollmentId { get; set; }
        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; }

        [Required]
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        // Useful flag if you ever need to know active/inactive subjects
        public bool IsSelected { get; set; } = true;
    }

}