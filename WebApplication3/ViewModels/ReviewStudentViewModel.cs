using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class ReviewStudentViewModel
    {
        public Student Student { get; set; }

        public List<EnrollmentDocument> Documents { get; set; }

        public List<PreviousResult> PreviousResults { get; set; } // Used for latest year display

        public int? LatestYear { get; set; }

        public double? AveragePercentage { get; set; }

        public int FailedCount { get; set; }

        public int PreviousGrade { get; set; }
    }


}