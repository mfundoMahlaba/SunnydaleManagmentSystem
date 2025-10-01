using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;



namespace WebApplication3.Models
{
    
    public class EnrollmentDocument
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public string DocumentType { get; set; } // "PreviousResults", "IDDocument"
        public string Status { get; set; } = "Pending Review"; // Options: "Pending", "Approved", "Rejected"
        public bool IsNotified { get; set; } = false;
        public string AdminComment { get; set; } // Optional feedback
        public DateTime UploadedAt { get; set; }

    }

}