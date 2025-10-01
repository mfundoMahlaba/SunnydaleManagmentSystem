using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class GradeSubject
    {
        [Key]
        public int Id { get; set; }
        public int Grade { get; set; }
        public int SubjectId { get; set; }

        public virtual Subject Subject { get; set; }
    }

}