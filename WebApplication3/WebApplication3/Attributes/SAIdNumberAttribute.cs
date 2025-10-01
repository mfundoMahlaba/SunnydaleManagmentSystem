using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Attributes
{
    public class SAIdNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string idNumber = value as string;
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13 || !idNumber.All(char.IsDigit))
                return false;

            // ✅ Extract and validate date of birth
            string dobPart = idNumber.Substring(0, 6);
            if (!TryParseDateOfBirth(dobPart, out DateTime dob))
                return false;

            // ✅ Validate citizenship digit (position 11, index 10)
            int citizenshipDigit = int.Parse(idNumber.Substring(10, 1));
            if (citizenshipDigit != 0 && citizenshipDigit != 1)
                return false;

            // ✅ Validate Luhn checksum
            if (!IsValidLuhn(idNumber))
                return false;

            return true;
        }

        private bool TryParseDateOfBirth(string dobPart, out DateTime dob)
        {
            dob = default;
            try
            {
                int year = int.Parse(dobPart.Substring(0, 2));
                int month = int.Parse(dobPart.Substring(2, 2));
                int day = int.Parse(dobPart.Substring(4, 2));

                // Determine century
                int currentYear = DateTime.Now.Year % 100;
                year += (year <= currentYear) ? 2000 : 1900;

                return DateTime.TryParse($"{year}-{month:D2}-{day:D2}", out dob);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidLuhn(string idNumber)
        {
            int sum = 0;
            for (int i = 0; i < idNumber.Length; i++)
            {
                int digit = int.Parse(idNumber[i].ToString());
                if (i % 2 == 0)
                {
                    sum += digit;
                }
                else
                {
                    int doubled = digit * 2;
                    sum += (doubled > 9) ? doubled - 9 : doubled;
                }
            }
            return sum % 10 == 0;
        }
    }
}