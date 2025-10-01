using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication23.Models
{
    public class Assign_SubjectsViewModel
    {
        public int TeacherId { get; set; }

        public List<SelectListItem> SubjectOptions { get; set; }

        public int[] SelectedSubjectIds { get; set; }
    }

}