using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class CaptureResultsViewModel
    {
        public int Grade { get; set; }
        public int Year { get; set; }
        public List<ResultEntry> Results { get; set; } = new List<ResultEntry>();
    }
    public class ResultEntry
    {
        public int SubjectId { get; set; }
        public decimal Percentage { get; set; }
    }
    // Add this class for validation
    public class YearValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Year is required");

            var year = (int)value;
            var currentYear = DateTime.Now.Year;

            if (year < 2023 || year > currentYear)
                return new ValidationResult($"Year must be between 2023 and {currentYear}");

            return ValidationResult.Success;
        }
    }
}