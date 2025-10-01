using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string HomeAddress { get; set; }

    }
}