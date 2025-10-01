using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class LearningMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public int UploadedByTeacherId { get; set; }
        public virtual Teacher UploadedByTeacher { get; set; }
    }

}