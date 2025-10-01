using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.ViewModels
{
    public class SendAnnouncementViewModel
    {
        public int SubjectId { get; set; }
        public int GradeId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public List<SelectListItem> SubjectOptions { get; set; }
        public List<SelectListItem> GradeOptions { get; set; }
    }

}