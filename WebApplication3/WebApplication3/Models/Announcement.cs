using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public int GradeId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }

        public virtual Teacher Teacher { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual Grade Grade { get; set; }
    }

}