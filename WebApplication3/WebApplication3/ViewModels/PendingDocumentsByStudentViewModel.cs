using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class PendingDocumentsByStudentViewModel
    {
        public Student Student { get; set; }
        public List<EnrollmentDocument> Documents { get; set; }
    }

}