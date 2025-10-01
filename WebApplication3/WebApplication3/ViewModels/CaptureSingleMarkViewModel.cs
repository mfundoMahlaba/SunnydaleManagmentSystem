using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class CaptureSingleMarkViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }

        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public int GradeId { get; set; }
        public string GradeName { get; set; }

        [Required(ErrorMessage = "Please select a term.")]
        public string SelectedTerm { get; set; }

        [Required(ErrorMessage = "Please select an assessment type.")]
        public string AssessmentType { get; set; }

        [Required(ErrorMessage = "Please enter a mark.")]
        [Range(0, 100, ErrorMessage = "Mark must be between 0 and 100.")]
        public double? Mark { get; set; }

        public IEnumerable<SelectListItem> Terms { get; set; }
        public IEnumerable<SelectListItem> AssessmentTypes { get; set; }

        // ✅ Add this to support overwrite confirmation and display
        public double? ExistingMark { get; set; }
    }
    public class StudentMarkOverviewViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public double? TestMark { get; set; }
        public double? ExamMark { get; set; }
        public double? AssignmentMark { get; set; }

        public double AverageMark
        {
            get
            {
                var marks = new List<double>();
                if (TestMark.HasValue) marks.Add(TestMark.Value);
                if (ExamMark.HasValue) marks.Add(ExamMark.Value);
                if (AssignmentMark.HasValue) marks.Add(AssignmentMark.Value);
                return marks.Any() ? marks.Average() : 0;
            }
        }
    }


}


