using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Classroom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g. "10A", "11B"

        [Required]
        public int GradeId { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }
        public int? StreamId { get; set; }
        public virtual Stream Stream { get; set; }
        public int Capacity { get; set; }
    }
}