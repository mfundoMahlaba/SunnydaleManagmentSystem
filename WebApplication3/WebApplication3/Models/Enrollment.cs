using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // The student enrolling
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Year of enrollment (e.g., 2025)
        [Required]
        public int Year { get; set; }

        // Grade the student is enrolling for (8–12)
        [Required]
        public int Grade { get; set; }

        // Stream only required for Grade 10–12
        public int? StreamId { get; set; }
        [ForeignKey("StreamId")]
        public virtual Stream Stream { get; set; }

        // New or Returning
        [Required]
        [MaxLength(20)]
        public string StudentType { get; set; } // "New" | "Returning"

        // The date the enrollment was created
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation property for selected subjects
        public virtual ICollection<EnrollmentSubject> EnrollmentSubjects { get; set; }
    }
    public class StudentSubjectEnrollment
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int GradeId { get; set; }
        public Grade Grade { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public string StudentType { get; set; }
        public int? StreamId { get; set; }
        public Stream Stream { get; set; }



    }
}