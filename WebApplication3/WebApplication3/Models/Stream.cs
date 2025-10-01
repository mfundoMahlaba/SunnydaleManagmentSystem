using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Stream
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Science", "Commerce"
        public virtual ICollection<Subject> Subjects { get; set; }
    }
    


}