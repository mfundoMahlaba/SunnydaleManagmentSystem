using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
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
using static WebApplication3.Controllers.TeacherController;
using Stream = WebApplication3.Models.Stream;


namespace WebApplication3.Controllers
{
    public class AdminController : Controller
    {
        private readonly SchoolDbContext _context;

        public AdminController()
        {
            _context = new SchoolDbContext();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            var model = new AdminSignupViewModel();
            return View(model);
        }

        // GET: Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(AdminSignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            var admin = new Admin
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();

            // ✅ Generate password reset token
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Email = admin.Email,
                Token = token,
                ExpiryDate = DateTime.Now.AddDays(2)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            // ✅ Compose reset link
            var resetLink = Url.Action("ResetPassword", "Admin", new { token = token }, protocol: Request.Url.Scheme);

            // ✅ Compose email
            var subject = "Welcome to Sunnydale Management";
            var body = $@"
Dear {admin.FullName},

Your admin account has been successfully created.

🔐 Login Details:
Email: {admin.Email}
Password: {model.Password}

To reset or change your password, click the link below:
{resetLink}

Please keep this information secure.

Warm regards,  
Sunnydale Admin Team
";

            SendEmail1(admin.Email, subject, body);

            TempData["Success"] = "Admin registered and email sent successfully.";
            return RedirectToAction("Signup");
        }

        private void SendEmail1(string toEmail, string subject, string body)
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


        // GET: Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);

            var admin = _context.Admins
                .FirstOrDefault(a => a.Email == model.Email);

            if (admin == null || admin.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            Session["AdminId"] = admin.Id;
            return RedirectToAction("FaceCheck");
        }
        //Verification Before Dashboard Access
        [HttpGet]
        public ActionResult FaceCheck()
        {
            if (Session["AdminId"] == null) return RedirectToAction("Login");

            int adminId = (int)Session["AdminId"];
            var admin = _context.Admins.Find(adminId);

            if (string.IsNullOrEmpty(admin.FaceToken))
                return RedirectToAction("FaceRegister");
            else
                return RedirectToAction("FaceVerify");
        }
        [HttpGet]
        public ActionResult FaceRegister() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FaceRegister(string imageBase64)
        {
            int adminId = (int)Session["AdminId"];
            var admin = _context.Admins.Find(adminId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var faceToken = await service.DetectFaceTokenAsync(imageBase64);
            if (faceToken == null) return Json(new { success = false, message = "No face detected" });

            admin.FaceToken = faceToken;
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
            int adminId = (int)Session["AdminId"];
            var admin = _context.Admins.Find(adminId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var confidence = await service.CompareToFaceTokenAsync(imageBase64, admin.FaceToken);

            if (confidence >= 75.0)
            {
                Session["IsFaceVerified"] = true;
                return Json(new { success = true, redirect = Url.Action("Dashboard") });
            }

            return Json(new { success = false, message = "Face not recognized. Try again." });
        }
        public ActionResult Dashboard()
        {
            if (Session["AdminId"] == null) return RedirectToAction("Login");

            int adminId = (int)Session["AdminId"];
            var admin = _context.Admins.Find(adminId);
            if (admin == null) return RedirectToAction("Login");

            var pendingDocs = _context.EnrollmentDocuments
                .Include(d => d.Student)
                .Where(d => d.Status == "Pending Review")
                .OrderByDescending(d => d.UploadedAt)
                .ToList();

            // 🧠 Set session values for layout
            Session["UserRole"] = "Admin";
            Session["fullName"] = admin.FullName;

            var model = new AdminDashboardViewModel
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                PendingDocuments = pendingDocs
            };

            return View(model);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Admin");
        }
        public string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        [HttpGet]
        public ActionResult AddStream() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStream(Stream model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool streamExists = _context.Streams
                .Any(s => s.Name.Trim().ToLower() == model.Name.Trim().ToLower());

            if (streamExists)
            {
                ModelState.AddModelError("Name", "A stream with this name already exists.");
                return View(model);
            }

            _context.Streams.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Stream added successfully.";
            return RedirectToAction("AddStream");
        }
        public ActionResult ReviewStudent(int studentId)
        {
            var student = _context.Students.Find(studentId);
            if (student == null)
                return HttpNotFound();

            // Get pending documents
            var documents = _context.EnrollmentDocuments
                .Where(d => d.StudentId == studentId && d.Status == "Pending Review")
                .ToList();

            // Find the latest year for this student's results
            int? latestYear = _context.PreviousResults
                .Where(r => r.StudentId == studentId)
                .Select(r => (int?)r.Year)
                .Max();

            // Only get results for the latest year
            var previousResults = new List<PreviousResult>();
            if (latestYear.HasValue)
            {
                previousResults = _context.PreviousResults
                    .Where(r => r.StudentId == studentId && r.Year == latestYear.Value)
                    .Include(r => r.Subject)
                    .ToList();
            }

            // Compute insights
            double? average = previousResults.Any() ? (double?)previousResults.Average(r => r.Percentage) : null;
            int failedCount = previousResults.Count(r => r.Percentage < 30);
            int previousGrade = previousResults.FirstOrDefault()?.Grade ?? 0;

            // Build view model
            var model = new ReviewStudentViewModel
            {
                Student = student,
                Documents = documents,
                PreviousResults = previousResults,
                LatestYear = latestYear,
                AveragePercentage = average,
                FailedCount = failedCount,
                PreviousGrade = previousGrade
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ApproveDocument(int docId, string comment)
        {
            var doc = _context.EnrollmentDocuments.Find(docId);
            if (doc != null)
            {
                doc.Status = "Approved";
                doc.AdminComment = comment;
                _context.SaveChanges();
            }

            return RedirectToAction("ReviewStudent", new { studentId = doc.StudentId });
        }
        [HttpPost]
        public ActionResult SendPromotionEmail(int studentId)
        {
            var student = _context.Students.Find(studentId);
            if (student == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(student.Email))
            {
                TempData["Error"] = "Student email address is missing. Please update their profile before sending.";
                return RedirectToAction("ReviewStudent", new { studentId });
            }

            bool allApproved = _context.EnrollmentDocuments
                .Where(d => d.StudentId == studentId)
                .All(d => d.Status == "Approved");

            int? latestYear = _context.PreviousResults
                .Where(r => r.StudentId == studentId)
                .Select(r => (int?)r.Year)
                .Max();

            var latestResults = latestYear.HasValue
                ? _context.PreviousResults
                    .Where(r => r.StudentId == studentId && r.Year == latestYear.Value)
                    .ToList()
                : new List<PreviousResult>();

            int failedCount = latestResults.Count(r => r.Percentage < 30);
            int previousGrade = latestResults.FirstOrDefault()?.Grade ?? 0;
            int nextGrade = previousGrade + 1;

            if (allApproved && failedCount <= 2)
            {
                string subject = $"Enrollment for Grade {nextGrade}";
                string body = $"Dear {student.FullName},\n\n" +
                              $"Congratulations! Based on your academic performance and approved documents, you are eligible to register for Grade {nextGrade}.\n\n" +
                              $"Please log in to your dashboard pay tuition fee and submit your enrollment.\n\n" +
                              $"Sunnydale Secondary School";

                try
                {
                    SendEmail(student.Email, subject, body);
                    TempData["Success"] = "Promotion email sent successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Failed to send email: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Student is not yet eligible for promotion.";
            }

            return RedirectToAction("ReviewStudent", new { studentId });
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email address is required.", nameof(toEmail));

            var message = new MailMessage
            {
                From = new MailAddress("mfundo.mahlabarh@gmail.com"), // You can also pull this from web.config
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("mfundo.mahlabarh@gmail.com", "yzzw vzjo staj osru")
            };

            smtp.Send(message);
        }


        [HttpPost]
        public ActionResult RejectDocument(int docId, string comment)
        {
            var doc = _context.EnrollmentDocuments.Find(docId);
            if (doc != null)
            {
                doc.Status = "Rejected";
                doc.AdminComment = comment;
                _context.SaveChanges();
            }
            return RedirectToAction("ReviewStudent", new { studentId = doc.StudentId });
        }

        public ActionResult PendingDocuments()
        {
            // Fetch all pending documents including student info
            var pendingDocs = _context.EnrollmentDocuments
                .Include(d => d.Student)
                .Where(d => d.Status == "Pending Review")
                .OrderByDescending(d => d.UploadedAt)
                .ToList();

            // Group by student
            var groupedByStudent = pendingDocs
                .GroupBy(d => d.Student)
                .Select(g => new PendingDocumentsByStudentViewModel
                {
                    Student = g.Key,
                    Documents = g.ToList()
                })
                .ToList();

            return View(groupedByStudent);
        }

        
        public ActionResult AddSubject()
        {
            var model = new SubjectViewModel
            {
                GradeOptions = _context.Grades
                    .Select(g => new SelectListItem
                    {
                        Value = g.Id.ToString(),
                        Text = g.Description // or $"{g.GradeLevel} - {g.Description}"
                    }).ToList(),

                StreamOptions = _context.Streams
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AddSubject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = new Subject
                {
                    Name = model.Name,
                    SubjectCode = model.SubjectCode,
                    StreamId = model.StreamId,
                    GradeId = model.GradeId
                };

                _context.Subjects.Add(subject);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Subject '{subject.Name}' was added successfully.";
                return RedirectToAction("AddSubject");
            }

            // 🔧 Re-populate dropdowns before returning view
            model.GradeOptions = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                }).ToList();

            model.StreamOptions = _context.Streams
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

            return View(model);
        }



        public ActionResult AssignSubjects(string teacherName, int? gradeId)
        {
            var teachersQuery = _context.Teachers.AsQueryable();
            var gradesQuery = _context.Grades.AsQueryable();

            if (!string.IsNullOrEmpty(teacherName))
                teachersQuery = teachersQuery.Where(t => t.FullName.Contains(teacherName));

            if (gradeId.HasValue)
                gradesQuery = gradesQuery.Where(g => g.Id == gradeId.Value);

            var model = new AssignSubjectsViewModel
            {
                Teachers = teachersQuery.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FullName
                }),
                Subjects = _context.Subjects
                    .ToList()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.SubjectCode} - {s.Name}"
                    }),
                Grades = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                }),
                FilterTeacherName = teacherName,
                FilterGradeId = gradeId
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AssignSubjects(AssignSubjectsViewModel model)
        {
            if (!_context.Teachers.Any(t => t.Id == model.SelectedTeacherId))
            {
                ModelState.AddModelError("", "Selected teacher does not exist.");
                model.Teachers = _context.Teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FullName
                });
                model.Subjects = _context.Subjects
                    .ToList()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.SubjectCode} - {s.Name}"
                    });
                model.Grades = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                });

                return View(model);
            }

            foreach (var subjectId in model.SelectedSubjectIds)
            {
                foreach (var gradeId in model.SelectedGradeIds)
                {
                    var exists = _context.TeacherSubjectAssignments.Any(a =>
                        a.TeacherId == model.SelectedTeacherId &&
                        a.SubjectId == subjectId &&
                        a.GradeId == gradeId);

                    if (!exists)
                    {
                        _context.TeacherSubjectAssignments.Add(new TeacherSubjectAssignment
                        {
                            TeacherId = model.SelectedTeacherId,
                            SubjectId = subjectId,
                            GradeId = gradeId
                        });
                    }
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Subjects assigned successfully.";
            return RedirectToAction("AssignSubjects");
        }
        public ActionResult ViewTeacherAssignments(string teacherName, int? gradeId)
        {
            var query = _context.TeacherSubjectAssignments
                .Include(a => a.Teacher)
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teacherName))
                query = query.Where(a => a.Teacher.FullName.Contains(teacherName));

            if (gradeId.HasValue)
                query = query.Where(a => a.GradeId == gradeId.Value);

            var assignments = query.Select(a => new TeacherAssignmentOverviewViewModel
            {
                AssignmentId = a.Id,
                TeacherName = a.Teacher.FullName,
                SubjectName = a.Subject.Name,
                SubjectCode = a.Subject.SubjectCode,
                GradeName = a.Grade.Description
            }).ToList();

            ViewBag.Grades = new SelectList(_context.Grades, "Id", "Description");
            ViewBag.FilterTeacherName = teacherName;
            ViewBag.FilterGradeId = gradeId;

            return View(assignments);
        }
        public ActionResult FilterAssignments(string teacherName, int? gradeId)
        {
            var query = _context.TeacherSubjectAssignments
                .Include(a => a.Teacher)
                .Include(a => a.Subject)
                .Include(a => a.Grade)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teacherName))
                query = query.Where(a => a.Teacher.FullName.Contains(teacherName));

            if (gradeId.HasValue)
                query = query.Where(a => a.GradeId == gradeId.Value);

            var filtered = query.Select(a => new TeacherAssignmentOverviewViewModel
            {
                AssignmentId = a.Id,
                TeacherName = a.Teacher.FullName,
                SubjectName = a.Subject.Name,
                SubjectCode = a.Subject.SubjectCode,
                GradeName = a.Grade.Description
            }).ToList();

            return PartialView("_AssignmentTable", filtered);
        }

        public ActionResult EditAssignment(int id)
        {
            var assignment = _context.TeacherSubjectAssignments.Find(id);
            if (assignment == null) return HttpNotFound();

            var model = new AssignSubjectsViewModel
            {
                SelectedTeacherId = assignment.TeacherId,
                SelectedSubjectIds = new List<int> { assignment.SubjectId },
                SelectedGradeIds = new List<int> { assignment.GradeId },
                Teachers = _context.Teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FullName
                }),
                Subjects = _context.Subjects
                    .ToList()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.SubjectCode} - {s.Name}"
                    }),
                Grades = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                })
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult EditAssignment(int id, AssignSubjectsViewModel model)
        {
            var assignment = _context.TeacherSubjectAssignments.Find(id);
            if (assignment == null) return HttpNotFound();

            assignment.TeacherId = model.SelectedTeacherId;
            assignment.SubjectId = model.SelectedSubjectIds.FirstOrDefault();
            assignment.GradeId = model.SelectedGradeIds.FirstOrDefault();

            _context.SaveChanges();
            TempData["Success"] = "Assignment updated successfully.";
            return RedirectToAction("ViewTeacherAssignments");
        }
        public ActionResult DeleteAssignment(int id)
        {
            var assignment = _context.TeacherSubjectAssignments.Find(id);
            if (assignment == null) return HttpNotFound();

            _context.TeacherSubjectAssignments.Remove(assignment);
            _context.SaveChanges();

            TempData["Success"] = "Assignment deleted successfully.";
            return RedirectToAction("ViewTeacherAssignments");
        }

        [HttpGet]
        public ActionResult CreateTimetable()
        {
            var model = new CreateTimetableViewModel
            {
                ClassroomOptions = _context.Classrooms
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList(),

                SubjectOptions = _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToList(),

                TeacherOptions = new List<SelectListItem>() // populated via AJAX
            };

            return View(model);
        }



        [HttpPost]
        public ActionResult CreateTimetable(CreateTimetableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all fields.";
                return RedirectToAction("CreateTimetable");
            }

            // 🔍 Enhanced conflict detection
            bool hasConflict = _context.ClassTimetables.Any(t =>
                t.DayOfWeek == model.DayOfWeek &&
                (
                    t.TimeSlot == model.TimeSlot &&
                    (t.TeacherId == model.TeacherId || t.ClassroomId == model.ClassroomId)
                ) ||
                (t.SubjectId == model.SubjectId && t.DayOfWeek == model.DayOfWeek)
            );

            if (hasConflict)
            {
                TempData["Error"] = "Conflict detected: overlapping slot or duplicate subject on the same day.";
                return RedirectToAction("CreateTimetable");
            }

            var entry = new ClassTimetable
            {
                ClassroomId = model.ClassroomId,
                SubjectId = model.SubjectId,
                TeacherId = model.TeacherId,
                DayOfWeek = model.DayOfWeek,
                TimeSlot = model.TimeSlot
            };

            _context.ClassTimetables.Add(entry);
            _context.SaveChanges();

            TempData["Success"] = "Timetable entry created successfully.";
            return RedirectToAction("ClassTimetable");
        }

        [HttpGet]
        public JsonResult GetTeachersBySubject(int subjectId)
        {
            var teachers = _context.TeacherSubjectAssignments
                .Where(a => a.SubjectId == subjectId)
                .Select(a => new
                {
                    Id = a.TeacherId,
                    Name = a.Teacher.FullName
                })
                .Distinct()
                .ToList();

            return Json(teachers, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ClassTimetable(int? classroomId)
        {
            // 🧠 Role-based filtering
            if (User.IsInRole("Student"))
            {
                classroomId = Convert.ToInt32(Session["ClassroomId"]);
            }

            var timetableQuery = _context.ClassTimetables
                .Include("Classroom")
                .Include("Subject")
                .Include("Teacher")
                .AsQueryable();

            // 🎯 Apply classroom filter
            if (classroomId.HasValue)
            {
                timetableQuery = timetableQuery.Where(t => t.ClassroomId == classroomId.Value);
            }

            // 🔐 Teacher view: show only their slots
            if (User.IsInRole("Teacher"))
            {
                int teacherId = Convert.ToInt32(Session["TeacherId"]);
                timetableQuery = timetableQuery.Where(t => t.TeacherId == teacherId);
            }

            var timetable = timetableQuery
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.TimeSlot)
                .Select(t => new TimetableEntryViewModel
                {
                    Id = t.Id,
                    ClassroomName = t.Classroom.Name,
                    SubjectName = t.Subject.Name,
                    TeacherName = t.Teacher.FullName,
                    DayOfWeek = t.DayOfWeek,
                    TimeSlot = t.TimeSlot
                })
                .ToList();

            // 📦 Classroom dropdown options
            ViewBag.ClassroomOptions = _context.Classrooms
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = classroomId.HasValue && c.Id == classroomId.Value
                })
                .ToList();

            return View(timetable);
        }

        [HttpGet]
        public ActionResult EditTimetable(int id)
        {
            var entry = _context.ClassTimetables.Find(id);
            if (entry == null) return HttpNotFound();

            var model = new CreateTimetableViewModel
            {
                ClassroomId = entry.ClassroomId,
                SubjectId = entry.SubjectId,
                TeacherId = entry.TeacherId,
                DayOfWeek = entry.DayOfWeek,
                TimeSlot = entry.TimeSlot
            };

            ViewBag.ClassroomOptions = new SelectList(_context.Classrooms, "Id", "Name", entry.ClassroomId);
            ViewBag.SubjectOptions = new SelectList(_context.Subjects, "Id", "Name", entry.SubjectId);
            ViewBag.TeacherOptions = new SelectList(_context.Teachers, "Id", "FullName", entry.TeacherId);

            ViewBag.EntryId = id;
            return View("CreateTimetable", model); // Reuse the same view
        }

        [HttpPost]
        public ActionResult EditTimetable(int id, CreateTimetableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all fields.";
                return RedirectToAction("EditTimetable", new { id });
            }

            var entry = _context.ClassTimetables.Find(id);
            if (entry == null) return HttpNotFound();

            // Optional: conflict check
            bool hasConflict = _context.ClassTimetables.Any(t =>
                t.Id != id &&
                t.DayOfWeek == model.DayOfWeek &&
                t.TimeSlot == model.TimeSlot &&
                (t.TeacherId == model.TeacherId || t.ClassroomId == model.ClassroomId));

            if (hasConflict)
            {
                TempData["Error"] = "Conflict detected: overlapping slot for teacher or classroom.";
                return RedirectToAction("EditTimetable", new { id });
            }

            entry.ClassroomId = model.ClassroomId;
            entry.SubjectId = model.SubjectId;
            entry.TeacherId = model.TeacherId;
            entry.DayOfWeek = model.DayOfWeek;
            entry.TimeSlot = model.TimeSlot;

            _context.SaveChanges();
            TempData["Success"] = "Timetable entry updated.";
            return RedirectToAction("ClassTimetable");
        }

        [HttpPost]
        public ActionResult DeleteTimetable(int id)
        {
            var entry = _context.ClassTimetables.Find(id);
            if (entry == null) return HttpNotFound();

            _context.ClassTimetables.Remove(entry);
            _context.SaveChanges();

            TempData["Success"] = "Timetable entry deleted.";
            return RedirectToAction("ClassTimetable");
        }

        // ===== Uniform Shop Admin =====

        private string UploadFolderVirtual => "~/Upload";
        private string UploadFolderPhysical => System.Web.HttpContext.Current.Server.MapPath("~/Upload");

        // List all items
        public ActionResult UniformItems()
        {
            var items = _context.UniformItems
                                .Include(i => i.Category) // 👈 Ensures Category is loaded
                                .OrderBy(i => i.Name)
                                .ToList();

            return View(items);
        }

        // Create item
        [HttpGet]
        public ActionResult CreateUniformItem()
        {
            ViewBag.Categories = new SelectList(_context.UniformCategories.OrderBy(c => c.Name), "Id", "Name");
            return View(new UniformItem { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUniformItem(UniformItem model, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.UniformCategories.OrderBy(c => c.Name), "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Ensure upload folder exists
            if (!Directory.Exists(UploadFolderPhysical))
                Directory.CreateDirectory(UploadFolderPhysical);

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var savePath = Path.Combine(UploadFolderPhysical, fileName);
                imageFile.SaveAs(savePath);
                model.ImagePath = VirtualPathUtility.ToAbsolute($"{UploadFolderVirtual}/{fileName}");
            }

            _context.UniformItems.Add(model);
            _context.SaveChanges();

            TempData["Message"] = "Uniform item created.";
            return RedirectToAction("UniformItems");
        }

        // Edit item
        [HttpGet]
        public ActionResult EditUniformItem(int id)
        {
            var item = _context.UniformItems.Find(id);
            if (item == null) return HttpNotFound();

            ViewBag.Categories = new SelectList(_context.UniformCategories.OrderBy(c => c.Name), "Id", "Name", item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUniformItem(UniformItem model, HttpPostedFileBase imageFile)
        {
            var item = _context.UniformItems.Find(model.Id);
            if (item == null) return HttpNotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.UniformCategories.OrderBy(c => c.Name), "Id", "Name", model.CategoryId);
                return View(model);
            }

            item.Name = model.Name;
            item.Description = model.Description;
            item.Price = model.Price;
            item.Stock = model.Stock;
            item.IsActive = model.IsActive;
            item.CategoryId = model.CategoryId;

            if (!Directory.Exists(UploadFolderPhysical))
                Directory.CreateDirectory(UploadFolderPhysical);

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var savePath = Path.Combine(UploadFolderPhysical, fileName);
                imageFile.SaveAs(savePath);
                item.ImagePath = VirtualPathUtility.ToAbsolute($"{UploadFolderVirtual}/{fileName}");
            }

            _context.SaveChanges();
            TempData["Message"] = "Uniform item updated.";
            return RedirectToAction("UniformItems");
        }

        // Delete item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUniformItem(int id)
        {
            var item = _context.UniformItems.Find(id);
            if (item == null) return HttpNotFound();

            _context.UniformItems.Remove(item);
            _context.SaveChanges();

            TempData["Message"] = "Uniform item deleted.";
            return RedirectToAction("UniformItems");
        }

        // Categories
        public ActionResult UniformCategories()
        {
            var cats = _context.UniformCategories.OrderBy(c => c.Name).ToList();
            return View(cats);
        }

        [HttpGet]
        public ActionResult CreateUniformCategory() => View(new UniformCategory());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUniformCategory(UniformCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.UniformCategories.Add(model);
            _context.SaveChanges();
            return RedirectToAction("UniformCategories");
        }

        [HttpGet]
        public ActionResult EditUniformCategory(int id)
        {
            var cat = _context.UniformCategories.Find(id);
            if (cat == null) return HttpNotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUniformCategory(UniformCategory model)
        {
            var cat = _context.UniformCategories.Find(model.Id);
            if (cat == null) return HttpNotFound();
            if (!ModelState.IsValid) return View(model);

            cat.Name = model.Name;
            _context.SaveChanges();
            return RedirectToAction("UniformCategories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUniformCategory(int id)
        {
            var cat = _context.UniformCategories.Find(id);
            if (cat == null) return HttpNotFound();
            _context.UniformCategories.Remove(cat);
            _context.SaveChanges();
            return RedirectToAction("UniformCategories");
        }

        // Orders oversight
        public ActionResult UniformOrders(string status = null)
        {
            var q = _context.UniformOrders
                            .Include(o => o.Parent) // Eager load Parent
                            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(o => o.Status == status);

            var orders = q.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public ActionResult UniformOrderDetails(int id)
        {
            var order = _context.UniformOrders
                .Include(o => o.Parent) // load parent
                .Include(o => o.Items.Select(i => i.UniformItem)) // load UniformItems inside items
                .SingleOrDefault(o => o.Id == id);

            if (order == null) return HttpNotFound();

            return View(order);
        }


        public ActionResult AddClassroom()
        {
            ViewBag.Grades = new SelectList(_context.Grades.OrderBy(g => g.GradeLevel), "Id", "Description");
            ViewBag.Streams = new SelectList(_context.Streams, "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddClassroom(Classroom model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Grades = new SelectList(_context.Grades.OrderBy(g => g.GradeLevel), "Id", "Description");
                ViewBag.Streams = new SelectList(_context.Streams, "Id", "Name");
                return View(model);
            }

            _context.Classrooms.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Classroom added successfully.";

            return RedirectToAction("AddClassroom"); // or wherever you assign streams
        }

        public ActionResult EditStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            ViewBag.Grades = new SelectList(_context.Grades.OrderBy(g => g.GradeLevel), "Id", "Description", student.GradeId);
            return View(student);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudent(Student model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Grades = new SelectList(_context.Grades.OrderBy(g => g.GradeLevel), "Id", "Description", model.GradeId);
                return View(model);
            }

            var student = _context.Students.Find(model.Id);
            if (student == null)
                return HttpNotFound();

            student.FullName = model.FullName;
            student.IDNumber = model.IDNumber;
            student.PhoneNumber = model.PhoneNumber;
            student.Email = model.Email;
            
            _context.SaveChanges();
            return RedirectToAction("StudentList");
        }

        public ActionResult DeleteStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            return View(student);
        }

        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            _context.Students.Remove(student);
            _context.SaveChanges();

            return RedirectToAction("StudentList");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new AdminForgotPasswordViewModel());
        }

        [HttpPost]
        public ActionResult ForgotPassword(AdminForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _context.Admins.FirstOrDefault(a => a.Email == model.Email);
            if (admin == null)
            {
                TempData["Message"] = "No admin account found with that email.";
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

            string resetLink = Url.Action("ResetPassword", "Admin", new { token = token }, Request.Url.Scheme);

            try
            {
                var mail = new MailMessage();
                mail.To.Add(model.Email);
                mail.Subject = "Password Reset Request";
                mail.Body = $"Hi {admin.FullName},\n\nClick the link below to reset your password:\n{resetLink}\n\nThis link will expire in 1 hour.";
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

            return RedirectToAction("ForgotPassword");
        }
        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword");
            }

            var vm = new AdminResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        public ActionResult ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == model.Token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword");
            }

            var admin = _context.Admins.FirstOrDefault(a => a.Email == resetEntry.Email);
            if (admin == null)
            {
                TempData["Message"] = "Admin account not found.";
                return RedirectToAction("ForgotPassword");
            }

            admin.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            _context.PasswordResetTokens.Remove(resetEntry);
            _context.SaveChanges();

            TempData["Message"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }


        public ActionResult StudentList()
        {
            var students = _context.Students.Include(s => s.Grade).OrderBy(s => s.FullName).ToList();
            return View(students);
        }
     

    }
}