using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class SubjectViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SubjectCode { get; set; }

        [Required]
        public int GradeId { get; set; }

        
        public int? StreamId { get; set; }

        public IEnumerable<SelectListItem> GradeOptions { get; set; }
        public IEnumerable<SelectListItem> StreamOptions { get; set; }
    }

}