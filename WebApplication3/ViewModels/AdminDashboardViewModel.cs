using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class AdminDashboardViewModel
    {

        public int Id { get; set; }
        [MinLength(13), MaxLength(13)]

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<EnrollmentDocument> PendingDocuments { get; set; }
        //public Dictionary<int, List<Subject>> EnrolledSubjectsMap { get; set; }


    }
}