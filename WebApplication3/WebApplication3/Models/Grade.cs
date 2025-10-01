using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 12)]
        public int GradeLevel { get; set; } // e.g. 8, 9, 10, 11, 12

        [Required]
        public string Description { get; set; } // e.g. "Grade 10 - Senior Phase"

        
    }
    
}