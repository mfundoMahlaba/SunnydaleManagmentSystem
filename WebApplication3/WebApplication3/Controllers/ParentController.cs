using Rotativa;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.ViewModels;
using static WebApplication3.Controllers.TeacherController;

namespace WebApplication3.Controllers
{
    public class ParentController : Controller
    {
        private readonly SchoolDbContext _context;

        public ParentController()
        {
            _context = new SchoolDbContext();
        }

        // GET: Signup
        [HttpGet]
        public ActionResult Signup() => View();

        // POST: Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(ParentSignupViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if the student ID exists
            bool studentExists = _context.Students
                .Any(s => s.IDNumber == model.LinkedStudentID);

            if (!studentExists)
            {
                ModelState.AddModelError("LinkedStudentID", "The Student ID you have provided is not linked with any student on our school.");
                return View(model);
            }

            var parent = new Parent
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                LinkedStudentID = model.LinkedStudentID,
                PhoneNumber = model.PhoneNumber
            };

            _context.Parents.Add(parent);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            int parentId = Convert.ToInt32(Session["ParentId"]);
            var parent = _context.Parents.Find(parentId);

            if (parent == null)
            {
                TempData["Error"] = "Parent not found.";
                return RedirectToAction("Login", "Parent");
            }

            var vm = new ParentProfileViewModel
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Email = parent.Email,
                PhoneNumber = parent.PhoneNumber
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult EditProfile(ParentProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var parent = _context.Parents.Find(model.Id);
            if (parent == null)
            {
                TempData["Error"] = "Parent not found.";
                return RedirectToAction("Login", "Parent");
            }

            parent.FullName = model.FullName;
            parent.Email = model.Email;
            parent.PhoneNumber = model.PhoneNumber;

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("EditProfile", "Parent");
        }



        // GET: Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(ParentLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);

            var parent = _context.Parents
                .FirstOrDefault(p => p.Email == model.Email);

            if (parent == null || parent.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            Session["ParentId"] = parent.Id;
            return RedirectToAction("FaceCheck");
        }
        [HttpGet]
        public ActionResult FaceCheck()
        {
            if (Session["ParentId"] == null) return RedirectToAction("Login");

            int parentId = (int)Session["ParentId"];
            var parent = _context.Parents.Find(parentId);

            if (string.IsNullOrEmpty(parent.FaceToken))
                return RedirectToAction("FaceRegister");
            else
                return RedirectToAction("FaceVerify");
        }
        //Check Face Before Login
        [HttpGet]
        public ActionResult FaceRegister() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FaceRegister(string imageBase64)
        {
            int parentId = (int)Session["ParentId"];
            var parent = _context.Parents.Find(parentId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var faceToken = await service.DetectFaceTokenAsync(imageBase64);
            if (faceToken == null) return Json(new { success = false, message = "No face detected" });

            parent.FaceToken = faceToken;
            _context.SaveChanges();

            Session.Clear(); // force logout
            return Json(new { success = true, message = "Face registered. Please login again." });
        }

        [HttpGet]
        public ActionResult FaceVerify() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FaceVerify(string imageBase64)
        {
            int parentId = (int)Session["ParentId"];
            var parent = _context.Parents.Find(parentId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var confidence = await service.CompareToFaceTokenAsync(imageBase64, parent.FaceToken);

            if (confidence >= 75.0)
            {
                Session["IsFaceVerified"] = true;
                return Json(new { success = true, redirect = Url.Action("Dashboard") });
            }

            return Json(new { success = false, message = "Face not recognized. Try again." });
        }

        public ActionResult Dashboard()
        {
            var parentId = (int)Session["ParentId"];
            var parent = _context.Parents.Find(parentId);

            // 🧠 Set session values for layout
            Session["UserRole"] = "Parent";
            Session["fullName"] = parent.FullName;
            return View(parent);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Parent");
        }
        private string HashPassword(string password)
        {
            return password; // Placeholder
        }

        public ActionResult MyOrders()
        {
            if (Session["ParentId"] == null)
                return RedirectToAction("Login", "Parent");

            int parentId = (int)Session["ParentId"];
            var orders = _context.UniformOrders
                                 .Where(o => o.ParentId == parentId)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            return View(orders);
        }

        // 📄 View Report Card
        public ActionResult ReportCard()
        {
            int parentId = Convert.ToInt32(Session["ParentId"]);
            var parent = _context.Parents.FirstOrDefault(p => p.Id == parentId);

            if (parent == null || string.IsNullOrEmpty(parent.LinkedStudentID))
            {
                TempData["Error"] = "Parent or linked student not found.";
                return RedirectToAction("Login", "Parent");
            }

            var student = _context.Students
                .Include("Grade")
                .FirstOrDefault(s => s.IDNumber == parent.LinkedStudentID);

            if (student == null)
            {
                TempData["Error"] = "No student found with the provided ID number.";
                return RedirectToAction("Login", "Parent");
            }

            ViewBag.StudentName = student.FullName;
            ViewBag.StudentId = student.Id;

            var marks = _context.Marks
                .Include("Subject")
                .Include("Grade")
                .Where(m => m.StudentId == student.Id)
                .ToList();

            var grouped = marks
                .GroupBy(m => new { m.SubjectId, m.GradeId, m.Term })
                .Select(g =>
                {
                    var first = g.First();
                    return new StudentReportCardViewModel
                    {
                        SubjectName = first.Subject.Name,
                        GradeName = first.Grade.Description,
                        Term = first.Term,
                        TestMark = g.FirstOrDefault(m => m.AssessmentType == "Test")?.Score,
                        ExamMark = g.FirstOrDefault(m => m.AssessmentType == "Exam")?.Score,
                        AssignmentMark = g.FirstOrDefault(m => m.AssessmentType == "Assignment")?.Score
                    };
                }).ToList();

            return View(grouped);
        }

        // 🧾 Export Report Card to PDF
        public ActionResult ExportToPdf()
        {
            int parentId = Convert.ToInt32(Session["ParentId"]);
            var parent = _context.Parents.FirstOrDefault(p => p.Id == parentId);

            if (parent == null || string.IsNullOrEmpty(parent.LinkedStudentID))
            {
                TempData["Error"] = "Parent or linked student not found.";
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students
                .Include("Grade")
                .FirstOrDefault(s => s.IDNumber == parent.LinkedStudentID);

            if (student == null)
            {
                TempData["Error"] = "No student found with the provided ID number.";
                return RedirectToAction("Login", "Account");
            }

            var marks = _context.Marks
                .Include("Subject")
                .Include("Grade")
                .Where(m => m.StudentId == student.Id)
                .ToList();

            var grouped = marks
                .GroupBy(m => new { m.SubjectId, m.GradeId, m.Term })
                .Select(g =>
                {
                    var first = g.First();
                    return new StudentReportCardViewModel
                    {
                        SubjectName = first.Subject.Name,
                        GradeName = first.Grade.Description,
                        Term = first.Term,
                        TestMark = g.FirstOrDefault(m => m.AssessmentType == "Test")?.Score,
                        ExamMark = g.FirstOrDefault(m => m.AssessmentType == "Exam")?.Score,
                        AssignmentMark = g.FirstOrDefault(m => m.AssessmentType == "Assignment")?.Score
                    };
                }).ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.StudentId = student.Id;

            return new ViewAsPdf("ReportCardPdf", grouped)
            {
                FileName = "ProgressReport.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }

        public ActionResult ChildAttendance(DateTime? fromDate, DateTime? toDate, int? subjectId)
        {
            int parentId = Convert.ToInt32(Session["ParentId"]);
            var parent = _context.Parents.FirstOrDefault(p => p.Id == parentId);

            if (parent == null || string.IsNullOrEmpty(parent.LinkedStudentID))
            {
                TempData["Error"] = "Parent or linked student not found.";
                return RedirectToAction("Login", "Parent");
            }

            var student = _context.Students.FirstOrDefault(s => s.IDNumber == parent.LinkedStudentID);

            if (student == null)
            {
                TempData["Error"] = "No student found with the provided ID number.";
                return RedirectToAction("Login", "Parent");
            }

            var query = _context.Attendances.Where(a => a.StudentId == student.Id);

            if (subjectId.HasValue)
                query = query.Where(a => a.SubjectId == subjectId.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            var records = query
                .OrderByDescending(a => a.Date)
                .Select(a => new StudentAttendanceViewModel
                {
                    Date = a.Date,
                    SubjectName = a.Subject.SubjectCode + " - " + a.Subject.Name,
                    PeriodName = a.Period.Name,
                    IsPresent = a.IsPresent,
                    Remarks = a.Remarks
                }).ToList();

            var model = new StudentAttendanceFilterViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                SubjectId = subjectId,
                Records = records,
                SubjectOptions = _context.Subjects
                    .ToList()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.SubjectCode} - {s.Name}"
                    })
            };

            ViewBag.StudentName = student.FullName;
            ViewBag.StudentId = student.Id;

            return View(model);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var parent = _context.Parents.FirstOrDefault(p => p.Email == model.Email);
            if (parent == null)
            {
                TempData["Message"] = "No account found with that email.";
                return RedirectToAction("ForgotPassword");
            }

            string token = Guid.NewGuid().ToString();

            var resetToken = new PasswordResetToken
            {
                Email = model.Email,
                Token = token,
                ExpiryDate = DateTime.Now.AddHours(1)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            string resetLink = Url.Action("ResetPassword", "Parent", new { token = token }, Request.Url.Scheme);

            try
            {
                var mail = new MailMessage();
                mail.To.Add(model.Email);
                mail.Subject = "Password Reset Request";
                mail.Body = $"Hi {parent.FullName},\n\nClick the link below to reset your password:\n{resetLink}\n\nThis link will expire in 1 hour.";
                mail.IsBodyHtml = false;

                var smtp = new SmtpClient();
                smtp.Send(mail);


                smtp.Send(mail);

                TempData["Message"] = "A password reset link has been sent to your email.";
            }
            catch
            {
                TempData["Message"] = "Failed to send email. Please try again.";
            }

            return RedirectToAction("ForgotPassword","Parent");
        }

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Parent");
            }

            var vm = new ParentResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        public ActionResult ResetPassword(ParentResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == model.Token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Parent");
            }

            var parent = _context.Parents.FirstOrDefault(p => p.Email == resetEntry.Email);
            if (parent == null)
            {
                TempData["Message"] = "Parent account not found.";
                return RedirectToAction("ForgotPassword", "Parent");
            }

            parent.PasswordHash = (model.NewPassword);
            _context.PasswordResetTokens.Remove(resetEntry);
            _context.SaveChanges();

            TempData["Message"] = "Your password has been reset successfully.";
            return RedirectToAction("Login","Parent");
        }




    }
}
