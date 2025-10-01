using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }   // e.g., Mathematics, English
        public string SubjectCode { get; set; }
        public int? StreamId { get; set; }
        public virtual Stream Stream { get; set; }
        public int GradeId { get; set; }
        public virtual Grade Grade { get; set; }
    }
}