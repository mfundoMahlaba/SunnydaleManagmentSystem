using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class StreamSubject
    {
        [Key]
        public int Id { get; set; }
        public int StreamId { get; set; }
        public int SubjectId { get; set; }
        public virtual Stream Stream { get; set; }
        public virtual Subject Subject { get; set; }
    }
}