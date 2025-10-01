using System;
using System.Collections.Generic;
using System.Data.Entity; // Add this at the top with other using statements
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.ViewModels;


namespace WebApplication3.Controllers
{
    public class TeacherController : Controller
    {
        private readonly SchoolDbContext _context;

        public TeacherController()
        {
            _context = new SchoolDbContext();
        }
        public static class PasswordHelper
        {
            public static string HashPassword(string password)
            {
                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(password);
                    var hash = sha.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("mfundo.mahlabarh@gmail.com", "yzzw vzjo staj osru"),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress("mfundo.mahlabarh@gmail.com", "Sunnydale Admin"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);
            smtp.Send(message);
        }

        // GET: Signup
        [HttpGet]
        public ActionResult Signup()
        {
            return View(new TeacherSignupViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(TeacherSignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔐 Create teacher object
            var teacher = new Teacher
            {
                IdNumber = model.IdNumber,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                HomeAddress = model.HomeAddress,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password)
            };

            // 💾 Save teacher to database
            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            // ✅ Step 1: Generate password reset token
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Email = teacher.Email,
                Token = token,
                ExpiryDate = DateTime.Now.AddDays(2)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            // ✅ Step 2: Send welcome email with login + reset link
            var resetLink = Url.Action("ResetPassword", "Teacher", new { token = token }, protocol: Request.Url.Scheme);

            var subject = "Welcome to Sunnydale Management";
            var body = $@"
Dear {teacher.FullName},

Your Teacher account has been successfully created.

🔐 Login Details:
Email Address: {teacher.Email}
Password: {model.Password}

To reset your password or set a new one, click the link below:
{resetLink}

Please keep this information secure.

Warm regards,  
Sunnydale Admin Team
";

            SendEmail(teacher.Email, subject, body);

            TempData["Success"] = "Teacher registered and email sent successfully.";
            return RedirectToAction("Signup");
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(TeacherLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);

            var teacher = _context.Teachers
                .FirstOrDefault(t => t.Email == model.Email);

            if (teacher == null || teacher.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            Session["TeacherId"] = teacher.Id;
            return RedirectToAction("FaceCheck");
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["TeacherId"] == null)
            {
                TempData["Error"] = "Session expired. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            int teacherId = Convert.ToInt32(Session["TeacherId"]);
            var teacher = _context.Teachers.Find(teacherId);

            var vm = new TeacherProfileViewModel
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                HomeAddress = teacher.HomeAddress
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult EditProfile(TeacherProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var teacher = _context.Teachers.Find(model.Id);
            teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.PhoneNumber = model.PhoneNumber;
            teacher.HomeAddress = model.HomeAddress;

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("EditProfile");
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

            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == model.Email);
            if (teacher == null)
            {
                TempData["Message"] = "No account found with that email.";
                return RedirectToAction("ForgotPassword", "Teacher");
            }

            // 🔐 Generate secure token
            string token = Guid.NewGuid().ToString();

            // 🗃️ Save token to database
            var resetToken = new PasswordResetToken
            {
                Email = model.Email,
                Token = token,
                ExpiryDate = DateTime.Now.AddHours(1)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            // 📩 Build reset link
            string resetLink = Url.Action("ResetPassword", "Teacher", new { token = token }, Request.Url.Scheme);

            // ✉️ Send email via SMTP
            try
            {
                var mail = new MailMessage();
                mail.To.Add(model.Email);
                mail.Subject = "Password Reset Request";
                mail.Body = $"Hi {teacher.FullName},\n\nClick the link below to reset your password:\n{resetLink}\n\nThis link will expire in 1 hour.";
                mail.IsBodyHtml = false;

                var smtp = new SmtpClient();
                smtp.Send(mail);


                smtp.Send(mail);

                TempData["Message"] = "A password reset link has been sent to your email.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email. Please try again later.";
                // Optional: log error for debugging
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
            }

            return RedirectToAction("ForgotPassword", "Teacher");
        }

        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Teacher");
            }

            var vm = new TeacherResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        public ActionResult ResetPassword(TeacherResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == model.Token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Teacher");
            }

            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == resetEntry.Email);
            if (teacher == null)
            {
                TempData["Message"] = "Teacher account not found.";
                return RedirectToAction("ForgotPassword", "Teacher");
            }

            teacher.PasswordHash = (model.NewPassword);
            _context.PasswordResetTokens.Remove(resetEntry);
            _context.SaveChanges();

            TempData["Message"] = "Your password has been reset successfully.";
            return RedirectToAction("Login","Teacher");
        }

        [HttpGet]
        public ActionResult FaceCheck()
        {
            if (Session["TeacherId"] == null) return RedirectToAction("Login");

            int teacherId = (int)Session["TeacherId"];
            var teacher = _context.Teachers.Find(teacherId);

            if (string.IsNullOrEmpty(teacher.FaceToken))
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
            int teacherId = (int)Session["TeacherId"];
            var teacher = _context.Teachers.Find(teacherId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var faceToken = await service.DetectFaceTokenAsync(imageBase64);
            if (faceToken == null) return Json(new { success = false, message = "No face detected" });

            teacher.FaceToken = faceToken;
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
            int teacherId = (int)Session["TeacherId"];
            var teacher = _context.Teachers.Find(teacherId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var confidence = await service.CompareToFaceTokenAsync(imageBase64, teacher.FaceToken);

            if (confidence >= 75.0)
            {
                Session["IsFaceVerified"] = true;
                return Json(new { success = true, redirect = Url.Action("Dashboard") });
            }

            return Json(new { success = false, message = "Face not recognized. Try again." });
        }
        public ActionResult Dashboard()
        {
            if (Session["TeacherId"] == null) return RedirectToAction("Login");

            int teacherId = (int)Session["TeacherId"];
            var teacher = _context.Teachers.Find(teacherId);
            if (teacher == null) return RedirectToAction("Login");

            // 🧠 Set session values for layout
            Session["UserRole"] = "Teacher";
            Session["fullName"] = teacher.FullName;
            var model = new TeacherDashboardViewModel
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                HomeAddress = teacher.HomeAddress,
               // AssignedSubjects = assignedSubjects
            };

            return View(model);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Teacher");
        }
        private string HashPassword(string password)
        {
            return password; // Placeholder
        }
        public ActionResult MySubjects(string subjectName, string gradeName, string studentName, int? studentId)
        {
            if (Session["TeacherId"] == null)
            {
                TempData["Error"] = "Teacher session not found. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            int teacherId = Convert.ToInt32(Session["TeacherId"]);

            var assignments = _context.TeacherSubjectAssignments
                .Include("Subject")
                .Include("Grade")
                .Where(a => a.TeacherId == teacherId)
                .ToList();

            // 🎯 Prepare dropdown options
            ViewBag.SubjectOptions = assignments
                .Select(a => a.Subject.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.GradeOptions = assignments
                .Select(a => a.Grade.Description)
                .Where(desc => !string.IsNullOrEmpty(desc))
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            // 🧠 Persist selected filters
            ViewBag.SelectedSubject = subjectName;
            ViewBag.SelectedGrade = gradeName;
            ViewBag.SelectedStudentName = studentName;
            ViewBag.SelectedStudentId = studentId;

            var viewModels = new List<TeacherSubjectDashboardViewModel>();

            foreach (var assignment in assignments)
            {
                if (!string.IsNullOrEmpty(subjectName) &&
                    !assignment.Subject.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(gradeName) &&
                    !assignment.Grade.Description.Equals(gradeName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var enrolledStudents = _context.StudentSubjectEnrollments
                    .Include("Student")
                    .Where(e => e.SubjectId == assignment.SubjectId &&
                                e.GradeId == assignment.GradeId)
                    .ToList();

                if (!string.IsNullOrEmpty(studentName))
                {
                    enrolledStudents = enrolledStudents
                        .Where(e => e.Student.FullName != null &&
                                    e.Student.FullName.IndexOf(studentName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                if (studentId.HasValue)
                {
                    enrolledStudents = enrolledStudents
                        .Where(e => e.StudentId == studentId.Value)
                        .ToList();
                }

                var studentEntries = enrolledStudents.Select(e => new StudentMarkEntry
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    ExistingMark = _context.Marks
                        .Where(m => m.StudentId == e.StudentId &&
                                    m.SubjectId == assignment.SubjectId &&
                                    m.GradeId == assignment.GradeId &&
                                    m.TeacherId == teacherId)
                        .Select(m => (double?)m.Score)
                        .FirstOrDefault()
                }).ToList();

                if (studentEntries.Any())
                {
                    viewModels.Add(new TeacherSubjectDashboardViewModel
                    {
                        AssignmentId = assignment.Id,
                        SubjectName = assignment.Subject.Name,
                        SubjectCode = assignment.Subject.SubjectCode,
                        GradeName = assignment.Grade.Description,
                        Students = studentEntries
                    });
                }
            }

            return View(viewModels);
        }



        [HttpGet]
        public ActionResult CaptureMarks(int studentId, int assignmentId)
        {
            var assignment = _context.TeacherSubjectAssignments
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .FirstOrDefault(a => a.Id == assignmentId);

            var student = _context.Students.Find(studentId);

            if (assignment == null || student == null)
            {
                TempData["Error"] = "Invalid assignment or student.";
                return RedirectToAction("AssignedSubjects");
            }

            // Check if a mark already exists for this student/subject/grade
            var existingMark = _context.Marks.FirstOrDefault(m =>
                m.StudentId == student.Id &&
                m.SubjectId == assignment.SubjectId &&
                m.GradeId == assignment.GradeId);

            var model = new CaptureSingleMarkViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                SubjectId = assignment.SubjectId,
                SubjectName = assignment.Subject.Name,
                GradeId = assignment.GradeId,
                GradeName = assignment.Grade.Description,
                //ExistingMark = existingMark?.Score,
                Terms = new List<SelectListItem>
        {
            new SelectListItem { Value = "Term 1", Text = "Term 1" },
            new SelectListItem { Value = "Term 2", Text = "Term 2" },
            new SelectListItem { Value = "Term 3", Text = "Term 3" },
            new SelectListItem { Value = "Term 4", Text = "Term 4" }
        },
                AssessmentTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Test", Text = "Test" },
            new SelectListItem { Value = "Exam", Text = "Exam" },
            new SelectListItem { Value = "Assignment", Text = "Assignment" }
        }
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CaptureMarks(CaptureSingleMarkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Terms = new List<SelectListItem>
        {
            new SelectListItem { Value = "Term 1", Text = "Term 1" },
            new SelectListItem { Value = "Term 2", Text = "Term 2" },
            new SelectListItem { Value = "Term 3", Text = "Term 3" },
            new SelectListItem { Value = "Term 4", Text = "Term 4" }

        };

                model.AssessmentTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Test", Text = "Test" },
            new SelectListItem { Value = "Exam", Text = "Exam" },
            new SelectListItem { Value = "Assignment", Text = "Assignment" }
        };

                return View(model);
            }
            var teacherId = (int)Session["TeacherId"];

            var existingMark = _context.Marks.FirstOrDefault(m =>
                m.StudentId == model.StudentId &&
                m.SubjectId == model.SubjectId &&
                m.GradeId == model.GradeId &&
                m.Term == model.SelectedTerm &&
                m.AssessmentType == model.AssessmentType);

            if (existingMark != null)
            {
                existingMark.Score = model.Mark ?? 0;
                existingMark.TeacherId = teacherId;
                _context.Entry(existingMark).State = EntityState.Modified;
                TempData["Success"] = "Mark updated successfully.";
            }
            else
            {
                var mark = new Mark
                {
                    StudentId = model.StudentId,
                    SubjectId = model.SubjectId,
                    GradeId = model.GradeId,
                    Term = model.SelectedTerm,
                    AssessmentType = model.AssessmentType,
                    Score = model.Mark ?? 0,
                    TeacherId = teacherId
                };

                _context.Marks.Add(mark);
                TempData["Success"] = "Mark captured successfully.";
            }

            _context.SaveChanges();
            return RedirectToAction("MySubjects");
        }
        [HttpGet]
        public ActionResult EditMark(int studentId, int assignmentId)
        {
            var assignment = _context.TeacherSubjectAssignments
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .FirstOrDefault(a => a.Id == assignmentId);

            var student = _context.Students.Find(studentId);

            if (assignment == null || student == null)
            {
                TempData["Error"] = "Invalid assignment or student.";
                return RedirectToAction("MySubjects");
            }
            int teacherId = Convert.ToInt32(Session["TeacherId"]);

            var existingMark = _context.Marks.FirstOrDefault(n =>
                n.StudentId == studentId &&
                n.SubjectId == assignment.SubjectId &&
                n.GradeId == assignment.GradeId &&
                n.TeacherId == teacherId);

            
            var model = new CaptureSingleMarkViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                SubjectId = assignment.SubjectId,
                SubjectName = assignment.Subject.Name,
                GradeId = assignment.GradeId,
                GradeName = assignment.Grade.Description,
                SelectedTerm = existingMark?.Term,
                AssessmentType = existingMark?.AssessmentType,
                Mark = existingMark?.Score,
                Terms = new List<SelectListItem>
        {
            new SelectListItem { Value = "Term 1", Text = "Term 1" },
            new SelectListItem { Value = "Term 2", Text = "Term 2" },
            new SelectListItem { Value = "Term 3", Text = "Term 3" },
            new SelectListItem { Value = "Term 4", Text = "Term 4" }
        },
                AssessmentTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Test", Text = "Test" },
            new SelectListItem { Value = "Exam", Text = "Exam" },
            new SelectListItem { Value = "Assignment", Text = "Assignment" }
        }
            };

            return View("CaptureMarks", model); // Reuse the same view
        }


        public ActionResult MarkSummary(string term)
        {
            var groupedMarks = _context.Marks
                .Include(m => m.Subject)
                .Include(m => m.Grade)
                .Where(m => string.IsNullOrEmpty(term) || m.Term == term) // ✅ Filter by term
                .ToList()
                .GroupBy(m => new { m.SubjectId, m.GradeId, m.Term, m.AssessmentType });

            var summaries = groupedMarks.Select(g => new SubjectMarkSummaryViewModel
            {
                SubjectName = g.First().Subject.Name,
                GradeName = g.First().Grade.Description,
                Term = g.Key.Term,
                AssessmentType = g.Key.AssessmentType,
                TotalStudents = _context.StudentSubjectEnrollments.Count(e =>
                    e.SubjectId == g.Key.SubjectId && e.GradeId == g.Key.GradeId),
                MarksCaptured = g.Count(),
                AverageMark = g.Average(m => m.Score)
            }).ToList();

            ViewBag.SelectedTerm = term;
            ViewBag.TermOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "All Terms" },
        new SelectListItem { Value = "Term 1", Text = "Term 1" },
        new SelectListItem { Value = "Term 2", Text = "Term 2" },
        new SelectListItem { Value = "Term 3", Text = "Term 3" },
        new SelectListItem { Value = "Term 4", Text = "Term 4" }
    };

            return View(summaries);
        }
        public ActionResult StudentMarks(int assignmentId)
        {
            var assignment = _context.TeacherSubjectAssignments
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
            {
                TempData["Error"] = "Invalid subject assignment.";
                return RedirectToAction("MySubjects");
            }

            var studentMarks = _context.StudentSubjectEnrollments
                .Include(e => e.Student)
                .Where(e => e.SubjectId == assignment.SubjectId && e.GradeId == assignment.GradeId)
                .Select(e => new StudentMarkOverviewViewModel
                {
                    StudentId = e.Student.Id,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,

                    TestMark = _context.Marks
                        .Where(m => m.StudentId == e.Student.Id &&
                                    m.SubjectId == assignment.SubjectId &&
                                    m.GradeId == assignment.GradeId &&
                                    m.AssessmentType == "Test")
                        .Select(m => (int?)m.Score)
                        .FirstOrDefault(),

                    ExamMark = _context.Marks
                        .Where(m => m.StudentId == e.Student.Id &&
                                    m.SubjectId == assignment.SubjectId &&
                                    m.GradeId == assignment.GradeId &&
                                    m.AssessmentType == "Exam")
                        .Select(m => (int?)m.Score)
                        .FirstOrDefault(),

                    AssignmentMark = _context.Marks
                        .Where(m => m.StudentId == e.Student.Id &&
                                    m.SubjectId == assignment.SubjectId &&
                                    m.GradeId == assignment.GradeId &&
                                    m.AssessmentType == "Assignment")
                        .Select(m => (int?)m.Score)
                        .FirstOrDefault()
                })
                .ToList();

            ViewBag.SubjectName = assignment.Subject.Name;
            ViewBag.GradeName = assignment.Grade.Description;
            ViewBag.AssignmentId = assignment.Id;

            return View(studentMarks);
        }
        // GET: MarkAttendance
        public ActionResult MarkAttendance(int? gradeId)
        {
            if (Session["TeacherId"] == null)
                return RedirectToAction("Login", "Teacher");

            var students = _context.Students
                .Where(s => gradeId == null || s.GradeId == gradeId)
                .Select(s => new AttendanceEntryViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FullName
                }).ToList();

            var model = new AttendanceRegisterViewModel
            {
                Date = DateTime.Today,
                GradeId = gradeId ?? 0,
                Entries = students,
                GradeOptions = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                }),
                SubjectId = null,
                PeriodId = null
            };

            // ✅ Subject dropdown with SubjectCode
            ViewBag.Subjects = _context.Subjects
    .ToList() // ✅ Forces EF to execute the query
    .Select(s => new SelectListItem
    {
        Value = s.Id.ToString(),
        Text = $"{s.SubjectCode} - {s.Name}" // ✅ Now safe to use string interpolation
    });

            ViewBag.Periods = _context.Periods.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            });

            var teacherId = GetLoggedInTeacherId();

            var teacher = _context.Teachers
                .Where(t => t.Id == teacherId)
                .Select(t => new { t.Id, t.FullName })
                .FirstOrDefault();

            ViewBag.TeacherId = teacher.Id;
            ViewBag.TeacherName = teacher.FullName;



            return View(model);
        }

        [HttpPost]
        public ActionResult MarkAttendance(AttendanceRegisterViewModel model)
        {
            var teacherId = GetLoggedInTeacherId(); // Replace with your actual logic

            foreach (var entry in model.Entries)
            {
                var attendance = new Attendance
                {
                    StudentId = entry.StudentId,
                    Date = model.Date,
                    IsPresent = entry.IsPresent,
                    Remarks = entry.Remarks,
                    GradeId = model.GradeId,
                    SubjectId = model.SubjectId,
                    PeriodId = model.PeriodId,
                    MarkedByTeacherId = teacherId,
                    MarkedAt = DateTime.Now
                };
            
                var teacher = _context.Teachers
                    .Where(t => t.Id == teacherId)
                    .Select(t => new { t.Id, t.FullName })
                    .FirstOrDefault();


                _context.Attendances.Add(attendance);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Attendance saved successfully.";
            return RedirectToAction("MarkAttendance");
        }


        private int GetLoggedInTeacherId()
        {
            if (Session["TeacherId"] == null)
                throw new UnauthorizedAccessException("Teacher is not logged in.");

            return Convert.ToInt32(Session["TeacherId"]);
        }
        public ActionResult AttendanceHistory(int? gradeId, DateTime? fromDate, DateTime? toDate, int? subjectId)
        {
            var teacherId = GetLoggedInTeacherId();

            var query = _context.Attendances
                .Where(a => a.MarkedByTeacherId == teacherId);

            if (gradeId.HasValue)
                query = query.Where(a => a.GradeId == gradeId.Value);

            if (subjectId.HasValue)
                query = query.Where(a => a.SubjectId == subjectId.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            var records = query
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceHistoryViewModel
                {
                    Date = a.Date,
                    StudentName = a.Student.FullName,
                    IsPresent = a.IsPresent,
                    Remarks = a.Remarks,
                    SubjectName = a.Subject.SubjectCode + " - " + a.Subject.Name,
                    PeriodName = a.Period.Name,
                    MarkedBy = a.MarkedByTeacher.FullName
                }).ToList();

            var model = new AttendanceHistoryFilterViewModel
            {
                GradeId = gradeId,
                FromDate = fromDate,
                ToDate = toDate,
                SubjectId = subjectId,
                Records = records,
                GradeOptions = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                }),
                SubjectOptions = _context.Subjects
    .ToList()
    .Select(s => new SelectListItem
    {
        Value = s.Id.ToString(),
        Text = $"{s.SubjectCode} - {s.Name}"
    })
            };

            return View(model);
        }

        public ActionResult UploadMaterial()
        {
            int teacherId = GetLoggedInTeacherId();

            var model = new LearningMaterialUploadViewModel
            {
                SubjectOptions = GetTeacherSubjectOptions(teacherId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadMaterial(LearningMaterialUploadViewModel model)
        {
            int teacherId = GetLoggedInTeacherId();

            if (!ModelState.IsValid)
            {
                model.SubjectOptions = GetTeacherSubjectOptions(teacherId);
                return View(model);
            }

            // ✅ Ensure upload folder exists
            string folderPath = Server.MapPath("~/Uploads/LearningMaterials");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // ✅ Generate unique filename
            string extension = Path.GetExtension(model.File.FileName);
            string uniqueName = Guid.NewGuid().ToString() + extension;
            string fullPath = Path.Combine(folderPath, uniqueName);

            // ✅ Save file
            model.File.SaveAs(fullPath);

            var material = new LearningMaterial
            {
                Title = model.Title,
                Description = model.Description,
                FilePath = "/Uploads/LearningMaterials/" + uniqueName,
                UploadedAt = DateTime.Now,
                SubjectId = model.SubjectId,
                UploadedByTeacherId = teacherId
            };

            _context.LearningMaterials.Add(material);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Material uploaded successfully.";
            return RedirectToAction("UploadMaterial");
        }

        private IEnumerable<SelectListItem> GetTeacherSubjectOptions(int teacherId)
        {
            return _context.TeacherSubjectAssignments
                .Where(a => a.TeacherId == teacherId)
                .Select(a => a.Subject)
                .ToList()
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SubjectCode + " - " + s.Name
                });
        }

        public ActionResult MyMaterials()
        {
            int teacherId = GetLoggedInTeacherId();

            var materials = _context.LearningMaterials
                .Where(m => m.UploadedByTeacherId == teacherId)
                .OrderByDescending(m => m.UploadedAt)
                .Select(m => new LearningMaterialListViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    SubjectName = m.Subject.SubjectCode + " - " + m.Subject.Name,
                    FilePath = m.FilePath,
                    UploadedAt = m.UploadedAt
                })
                .ToList();

            return View(materials);
        }
        public ActionResult EditMaterial(int id)
        {
            var material = _context.LearningMaterials.Find(id);
            if (material == null || material.UploadedByTeacherId != GetLoggedInTeacherId())
                return HttpNotFound();

            var model = new LearningMaterialUploadViewModel
            {
                Title = material.Title,
                Description = material.Description,
                SubjectId = material.SubjectId,
                SubjectOptions = GetTeacherSubjectOptions(material.UploadedByTeacherId)
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMaterial(int id, LearningMaterialUploadViewModel model)
        {
            var material = _context.LearningMaterials.Find(id);
            if (material == null || material.UploadedByTeacherId != GetLoggedInTeacherId())
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                model.SubjectOptions = GetTeacherSubjectOptions(material.UploadedByTeacherId);
                return View(model);
            }

            material.Title = model.Title;
            material.Description = model.Description;
            material.SubjectId = model.SubjectId;

            if (model.File != null)
            {
                string folderPath = Server.MapPath("~/Uploads/LearningMaterials");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                string fullPath = Path.Combine(folderPath, uniqueName);
                model.File.SaveAs(fullPath);

                material.FilePath = "/Uploads/LearningMaterials/" + uniqueName;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Material updated successfully.";
            return RedirectToAction("MyMaterials");
        }
        public ActionResult DeleteMaterial(int id)
        {
            var material = _context.LearningMaterials.Find(id);
            if (material == null || material.UploadedByTeacherId != GetLoggedInTeacherId())
                return HttpNotFound();

            _context.LearningMaterials.Remove(material);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Material deleted successfully.";
            return RedirectToAction("MyMaterials");
        }
        public ActionResult SubjectMarks(string term, string assessmentType, string studentName, int? studentId, string subjectName)
        {
            int teacherId = Convert.ToInt32(Session["TeacherId"]);

            var assignments = _context.TeacherSubjectAssignments
                .Include("Subject")
                .Include("Grade")
                .Where(a => a.TeacherId == teacherId)
                .ToList();

            var viewModels = new List<TeacherSubjectMarkViewModel>();

            foreach (var assignment in assignments)
            {
                if (!string.IsNullOrEmpty(subjectName) && !assignment.Subject.Name.Contains(subjectName))
                    continue;

                var marks = _context.Marks
                    .Include("Student")
                    .Where(m => m.SubjectId == assignment.SubjectId &&
                                m.GradeId == assignment.GradeId &&
                                m.TeacherId == teacherId)
                    .ToList();

                if (!string.IsNullOrEmpty(term))
                    marks = marks.Where(m => m.Term == term).ToList();

                if (!string.IsNullOrEmpty(assessmentType))
                    marks = marks.Where(m => m.AssessmentType == assessmentType).ToList();

                if (!string.IsNullOrEmpty(studentName))
                    marks = marks.Where(m => m.Student.FullName.Contains(studentName)).ToList();

                if (studentId.HasValue)
                    marks = marks.Where(m => m.StudentId == studentId.Value).ToList();

                var studentEntries = marks.Select(m => new StudentMarkEntry01
                {
                    StudentId = m.StudentId,
                    StudentName = m.Student.FullName,
                    Term = m.Term,
                    AssessmentType = m.AssessmentType,
                    Mark = m.Score
                }).ToList();

                if (studentEntries.Any())
                {
                    viewModels.Add(new TeacherSubjectMarkViewModel
                    {
                        SubjectName = assignment.Subject.Name,
                        GradeName = assignment.Grade.Description,
                        Students = studentEntries
                    });
                }
            }

            return View(viewModels);
        }
        public ActionResult ViewMyTimetable()
        {
            var teacherIdObj = Session["TeacherId"];
            if (teacherIdObj == null)
            {
                TempData["Error"] = "You must be logged in to view your timetable.";
                return RedirectToAction("Login", "Account");
            }

            int teacherId = Convert.ToInt32(teacherIdObj);

            var timetable = _context.ClassTimetables
                .Include(t => t.Subject)
                .Include(t => t.Subject.Grade) // if Grade is nested under Subject
                .Include(t => t.Classroom)
                .Where(t => t.TeacherId == teacherId)
                .Select(t => new TeacherTimetableViewModel
                {
                    SubjectName = t.Subject.Name,
                    GradeName = t.Subject.Grade.Description,
                    ClassroomName = t.Classroom.Name,
                    DayOfWeek = t.DayOfWeek,
                    TimeSlot = t.TimeSlot
                })
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.TimeSlot)
                .ToList();

            ViewBag.TeacherName = _context.Teachers.Find(teacherId)?.FullName;

            return View(timetable);
        }
        [HttpGet]
        public ActionResult SendAnnouncement()
        {
            if (Session["TeacherId"] == null)
            {
                TempData["Error"] = "Teacher session not found. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            int teacherId = (int)Session["TeacherId"];

            var assignments = _context.TeacherSubjectAssignments
                .Where(a => a.TeacherId == teacherId)
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .ToList();

            var model = new SendAnnouncementViewModel
            {
                SubjectOptions = assignments
                    .Select(a => new SelectListItem
                    {
                        Value = a.SubjectId.ToString(),
                        Text = a.Subject.Name
                    }).Distinct().ToList(),

                GradeOptions = assignments
                    .Select(a => new SelectListItem
                    {
                        Value = a.GradeId.ToString(),
                        Text = a.Grade.Description
                    }).Distinct().ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendAnnouncement(SendAnnouncementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("SendAnnouncement");
            }

            if (Session["TeacherId"] == null)
            {
                TempData["Error"] = "Teacher session not found. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            int teacherId = (int)Session["TeacherId"];

            var announcement = new Announcement
            {
                TeacherId = teacherId,
                SubjectId = model.SubjectId,
                GradeId = model.GradeId,
                Title = model.Title,
                Message = model.Message,
                SentAt = DateTime.Now
            };

            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            // ✅ Load subject and grade names for email
            var subject = _context.Subjects.Find(model.SubjectId);
            var grade = _context.Grades.Find(model.GradeId);

            var enrolledStudents = _context.StudentSubjectEnrollments
                .Where(e => e.SubjectId == model.SubjectId && e.GradeId == model.GradeId)
                .Include(e => e.Student)
                .ToList();

            foreach (var enrollment in enrolledStudents)
            {
                var student = enrollment.Student;
                if (student != null && !string.IsNullOrEmpty(student.Email))
                {
                    var emailSubject = $"📢 Announcement: {model.Title}";
                    var emailBody = $@"
Dear {student.FullName},

Your teacher has posted a new announcement for {subject?.Name} - {grade?.Description}.

📌 {model.Title}
{model.Message}


Sunnydale Secondary School
";

                    SendEmail2(student.Email, emailSubject, emailBody);
                }
            }

            TempData["Success"] = "Announcement sent and emails delivered.";
            return RedirectToAction("MySubjects");
        }

        private void SendEmail2(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("mfundo.mahlabarh@gmail.com", "yzzw vzjo staj osru"),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress("mfundo.mahlabarh@gmail.com", "Sunnydale Admin"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);
            smtp.Send(message);
        }



    }
}