using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class LearningMaterialUploadViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public HttpPostedFileBase File { get; set; }

        [Required]
        public int SubjectId { get; set; }

        public IEnumerable<SelectListItem> SubjectOptions { get; set; }
    }

    public class LearningMaterialListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
    public class StudentMaterialViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
    public class StudentMaterialFilterViewModel
    {
        public int? SubjectId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public IEnumerable<SelectListItem> SubjectOptions { get; set; }
        public List<StudentMaterialViewModel> Materials { get; set; }
    }




}