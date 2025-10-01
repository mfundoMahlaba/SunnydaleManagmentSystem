using PagedList;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.ViewModels;
using static WebApplication3.Controllers.TeacherController;
namespace WebApplication3.Controllers
{
    public class StudentController : Controller
    {
        private readonly SchoolDbContext _context;

        public StudentController()
        {
            _context = new SchoolDbContext();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            var model = new StudentSignupViewModel
            {
                GenderOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Male", Text = "Male" },
            new SelectListItem { Value = "Female", Text = "Female" },
            new SelectListItem { Value = "Other", Text = "Other" }
        },
                RaceOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Black", Text = "Black" },
            new SelectListItem { Value = "White", Text = "White" },
            new SelectListItem { Value = "Coloured", Text = "Coloured" },
            new SelectListItem { Value = "Indian/Asian", Text = "Indian/Asian" },
            new SelectListItem { Value = "Other", Text = "Other" }
        }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(StudentSignupViewModel model)
        {
            // Rebind dropdowns if validation fails
            void RebindDropdowns()
            {
                model.GenderOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Male", Text = "Male" },
            new SelectListItem { Value = "Female", Text = "Female" },
            new SelectListItem { Value = "Other", Text = "Other" }
        };

                model.RaceOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Black", Text = "Black" },
            new SelectListItem { Value = "White", Text = "White" },
            new SelectListItem { Value = "Coloured", Text = "Coloured" },
            new SelectListItem { Value = "Indian/Asian", Text = "Indian/Asian" },
            new SelectListItem { Value = "Other", Text = "Other" }
        };
            }

            if (!ModelState.IsValid)
            {
                RebindDropdowns();
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                RebindDropdowns();
                return View(model);
            }

            var student = new Student
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber,
                AlternativePhoneNumber = model.AlternativePhoneNumber,
                IDNumber = model.IDNumber,
                Gender = model.Gender,
                Race = model.Race,
                Address = model.Address,
                CreatedAt = DateTime.Now
            };

            _context.Students.Add(student);
            _context.SaveChanges();

            // ✅ Generate password reset token
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Email = student.Email,
                Token = token,
                ExpiryDate = DateTime.Now.AddDays(2)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            // ✅ Compose reset link
            var resetLink = Url.Action("ResetPassword", "Student", new { token = token }, protocol: Request.Url.Scheme);

            // ✅ Compose email
            var subject = "Welcome to Sunnydale Secondary School";
            var body = $@"
Dear {student.FullName},

Your student account has been successfully created.

🔐 Login Details:
Email: {student.Email}
Password: {model.Password}

To reset or change your password, click the link below:
{resetLink}

Please keep this information secure.

Warm regards,  
Sunnydale Admin Team
";

            SendEmail(student.Email, subject, body);

            return RedirectToAction("Login");
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



        [HttpGet]
        public ActionResult EditProfile()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            var student = _context.Students.Find(studentId);

            var vm = new StudentProfileViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
            };

            return View(vm);
        }
        [HttpPost]
        public ActionResult EditProfile(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = _context.Students.Find(model.Id);
            student.FullName = model.FullName;
            student.Email = model.Email;
            student.PhoneNumber = model.PhoneNumber;

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("EditProfile", "Student");
        }



        // GET: Student Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: Student Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StudentLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);

            var student = _context.Students
                .FirstOrDefault(s => s.Email == model.Email);

            if (student == null || student.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            Session["StudentId"] = student.Id;
            return RedirectToAction("FaceCheck");
        }

        [HttpGet]
        public ActionResult FaceCheck()
        {
            if (Session["StudentId"] == null) return RedirectToAction("Login");

            int studentId = (int)Session["StudentId"];
            var student = _context.Students.Find(studentId);

            if (string.IsNullOrEmpty(student.FaceToken))
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
            int studentId = (int)Session["StudentId"];
            var student = _context.Students.Find(studentId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var faceToken = await service.DetectFaceTokenAsync(imageBase64);
            if (faceToken == null) return Json(new { success = false, message = "No face detected" });

            student.FaceToken = faceToken;
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
            int studentId = (int)Session["StudentId"];
            var student = _context.Students.Find(studentId);

            var service = new FacePlusPlusService(
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_KEY"],
                System.Configuration.ConfigurationManager.AppSettings["FACEPP_SECRET"]
            );

            var confidence = await service.CompareToFaceTokenAsync(imageBase64, student.FaceToken);

            if (confidence >= 75.0)
            {
                Session["IsFaceVerified"] = true;
                return Json(new { success = true, redirect = Url.Action("Dashboard") });
            }

            return Json(new { success = false, message = "Face not recognized. Try again." });
        }

        // GET: Student Dashboard
        public ActionResult Dashboard()
        {
            // 🔐 Ensure student is logged in
            if (Session["StudentId"] == null)
                return RedirectToAction("Login", "Student");

            int studentId = (int)Session["StudentId"];

            // 🎓 Fetch student record
            var student = _context.Students.Find(studentId);
            if (student == null)
                return RedirectToAction("Login", "Student");

            // 📄 Fetch enrollment documents
            var documents = _context.EnrollmentDocuments
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.UploadedAt)
                .ToList();

            // 📌 Get latest document status
            var latestStatus = documents
                .Select(d => d.Status)
                .FirstOrDefault();

            // ✅ Check if required documents are approved
            var requiredTypes = new[] { "PreviousResults", "IDDocument" };
            bool areDocumentsApproved = requiredTypes.All(type =>
                documents.Any(d => d.DocumentType == type && d.Status == "Approved")
            );

            // 🧠 Set session values for layout
            Session["UserRole"] = "Student";
            Session["fullName"] = student.FullName;

            // 📦 Build dashboard view model
            var model = new StudentDashboardViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                IDNumber = student.IDNumber,
                Email = student.Email,
                HasPaid = HasPaidTuition(studentId),
                PhoneNumber = student.PhoneNumber,
                LatestDocumentStatus = latestStatus,
                CanPayFees = areDocumentsApproved
            };

            return View(model);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Student");
        }

        private string HashPassword(string password)
        {
            // Replace with actual hashing logic
            return password;
        }

        // Student Previouse Results
        [HttpGet]
        public ActionResult Capture()
        {
            if (Session["StudentId"] == null)
                return RedirectToAction("Login", "Student");

            int studentId = (int)Session["StudentId"];

            var capturedYears = _context.PreviousResults
                .Where(r => r.StudentId == studentId)
                .Select(r => r.Year)
                .Distinct()
                .ToList();

            ViewBag.CapturedYears = capturedYears;

            ViewBag.Grades = _context.Grades
                .OrderBy(g => g.GradeLevel)
                .Select(g => new { Id = g.GradeLevel, g.Description })
                .ToList();

            ViewBag.Subjects = _context.Subjects.ToList();

            var vm = new CaptureResultsViewModel
            {
                Results = new List<ResultEntry> { new ResultEntry() }
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult Capture(CaptureResultsViewModel model)
        {
            if (Session["StudentId"] == null)
                return RedirectToAction("Login", "Student");

            int studentId = (int)Session["StudentId"];

            bool alreadyCaptured = _context.PreviousResults
                .Any(r => r.StudentId == studentId && r.Year == model.Year);

            if (alreadyCaptured)
                ModelState.AddModelError("", $"You have already captured results for {model.Year}.");

            if (model.Results.Count < 7)
                ModelState.AddModelError("", "You must capture results for at least 7 subjects.");

            var duplicateSubjects = model.Results
                .GroupBy(r => r.SubjectId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSubjects.Any())
                ModelState.AddModelError("", "Duplicate subjects detected. Please select each subject only once.");

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _context.Subjects.ToList();
                return View(model);
            }

            foreach (var entry in model.Results)
            {
                var result = new PreviousResult
                {
                    StudentId = studentId,
                    Grade = model.Grade,
                    Year = model.Year,
                    SubjectId = entry.SubjectId,
                    Percentage = entry.Percentage
                };

                _context.PreviousResults.Add(result);
            }

            _context.SaveChanges();

            //TempData["Success"] = "Results captured successfully!";
            return RedirectToAction("MyResults");
        }

        public ActionResult MyResults()
        {
            int studentId = (int)Session["StudentId"];
            var results = _context.PreviousResults
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Subject)
                .OrderByDescending(r => r.Year)
                .ThenBy(r => r.Grade)
                .ToList();
            return View(results);
        }

        public ActionResult UploadDocuments()
        {
            if (Session["StudentId"] == null)
                return RedirectToAction("Login", "Student");

            return View();
        }

        [HttpPost]
        public ActionResult UploadDocuments(HttpPostedFileBase idDocument, HttpPostedFileBase reportDocument)
        {
            if (Session["StudentId"] == null)
                return RedirectToAction("Login", "Student");

            int studentId = (int)Session["StudentId"];
            int currentYear = DateTime.Now.Year;

            // Count uploads for this student in the current year
            int uploadsThisYear = _context.EnrollmentDocuments
                .Count(d => d.StudentId == studentId && d.UploadedAt.Year == currentYear);

            if (uploadsThisYear >= 4)
            {
                ModelState.AddModelError("", $"You have already uploaded {uploadsThisYear} documents for {currentYear}. Only 2 uploads are allowed per year.");
                return View();
            }

            bool hasValidUpload = false;

            // Upload ID Document
            if (idDocument != null)
            {
                if (!IsValidFile(idDocument))
                {
                    ModelState.AddModelError("", "ID document must be a PDF and less than 5MB.");
                }
                else
                {
                    var folder = Server.MapPath("~/Uploads/IDs");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Path.GetFileName(idDocument.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    idDocument.SaveAs(filePath);

                    _context.EnrollmentDocuments.Add(new EnrollmentDocument
                    {
                        StudentId = studentId,
                        FileName = fileName,
                        FilePath = "/Uploads/IDs/" + fileName,
                        DocumentType = "IDDocument",
                        Status = "Pending Review",
                        UploadedAt = DateTime.Now
                    });

                    hasValidUpload = true;
                }
            }

            // Upload Report Document
            if (reportDocument != null)
            {
                if (!IsValidFile(reportDocument))
                {
                    ModelState.AddModelError("", "Report must be a PDF and less than 5MB.");
                }
                else
                {
                    var folder = Server.MapPath("~/Uploads/Reports");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Path.GetFileName(reportDocument.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    reportDocument.SaveAs(filePath);

                    _context.EnrollmentDocuments.Add(new EnrollmentDocument
                    {
                        StudentId = studentId,
                        FileName = fileName,
                        FilePath = "/Uploads/Reports/" + fileName,
                        DocumentType = "PreviousResults",
                        Status = "Pending Review",
                        UploadedAt = DateTime.Now
                    });

                    hasValidUpload = true;
                }
            }

            if (!hasValidUpload)
            {
                ModelState.AddModelError("", "Please upload valid PDF documents only!");
                return View();
            }

            _context.SaveChanges();
            TempData["Message"] = "Documents uploaded successfully.";
            return RedirectToAction("UploadDocuments", "Student");
        }

        private bool IsValidFile(HttpPostedFileBase file)
        {
            if (file == null) return false;

            var allowedMime = "application/pdf";
            var extension = Path.GetExtension(file.FileName).ToLower();
            var maxSize = 5 * 1024 * 1024; // 5MB

            return file.ContentType == allowedMime && extension == ".pdf" && file.ContentLength <= maxSize;
        }


        public ActionResult DocumentOverview(int? page)
        {
            int studentId = (int)Session["StudentId"];
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var documents = _context.EnrollmentDocuments
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.UploadedAt)
                .ToPagedList(pageNumber, pageSize);

            return View(documents);
        }
        public ActionResult EnrollSubjects()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            if (!HasPaidTuition(studentId))
            {
                TempData["Error"] = "You must pay your tuition fee before submitting your enrollment!";
                return RedirectToAction("Dashboard", "Student");
            }

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

            double? average = previousResults.Any() ? (double?)previousResults.Average(r => r.Percentage) : null;
            int failedCount = previousResults.Count(r => r.Percentage < 30);
            int previousGrade = previousResults.FirstOrDefault()?.Grade ?? 0;

            var grades = _context.Grades.ToList();
            var model = new SubjectEnrollmentViewModel
            {
                Grades = grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                }),
                Streams = new List<SelectListItem>(),
                Subjects = new List<SelectListItem>(),
                Years = GetYears(),
                PreviousResults = previousResults,
                AveragePercentage = average,
                FailedCount = failedCount,
                PreviousGrade = previousGrade,
                StudentTypeList = GetStudentTypes()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult EnrollSubjects(SubjectEnrollmentViewModel model)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            if (!HasPaidTuition(studentId))
            {
                TempData["Error"] = "Enrollment blocked — payment not received.";
                return RedirectToAction("Dashboard", "Student");
            }

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

            double? average = previousResults.Any() ? (double?)previousResults.Average(r => r.Percentage) : null;
            int failedCount = previousResults.Count(r => r.Percentage < 30);
            int previousGrade = previousResults.FirstOrDefault()?.Grade ?? 0;

            model.PreviousResults = previousResults;
            model.AveragePercentage = average;
            model.FailedCount = failedCount;
            model.PreviousGrade = previousGrade;

            if (model.SelectedSubjectIds == null || model.SelectedSubjectIds.Count < 7)
            {
                ModelState.AddModelError("", "You must select at least 7 subjects.");
            }

            var existing = _context.StudentSubjectEnrollments
                .Where(e => e.StudentId == studentId && e.GradeId == model.SelectedGradeId)
                .Select(e => e.SubjectId)
                .ToList();

            var duplicates = model.SelectedSubjectIds?.Intersect(existing).ToList() ?? new List<int>();
            if (duplicates.Any())
            {
                ModelState.AddModelError("", "Some subjects are already enrolled for this grade.");
            }

            if (!ModelState.IsValid)
            {
                var grades = _context.Grades.ToList();
                var subjectsQuery = _context.Subjects.Where(s => s.GradeId == model.SelectedGradeId);

                if (model.SelectedStreamId.HasValue)
                {
                    subjectsQuery = subjectsQuery.Where(s => s.StreamId == model.SelectedStreamId.Value);
                }

                var subjects = subjectsQuery.ToList();
                var streams = (model.SelectedGradeId == 10 || model.SelectedGradeId == 11 || model.SelectedGradeId == 12)
                    ? _context.Streams.ToList()
                    : new List<WebApplication3.Models.Stream>();

                model.Grades = grades.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Description
                });

                model.Subjects = subjects.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.SubjectCode} - {s.Name}"
                });

                model.Streams = streams.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                });

                // FIX: Ensure Years is always set
                model.Years = GetYears();

                model.StudentTypeList = GetStudentTypes();

                return View(model);
            }

            foreach (var subjectId in model.SelectedSubjectIds)
            {
                _context.StudentSubjectEnrollments.Add(new StudentSubjectEnrollment
                {
                    StudentId = studentId,
                    SubjectId = subjectId,
                    GradeId = model.SelectedGradeId,
                    Year = model.SelectedYear,
                    StudentType = model.SelectedStudentType 
                });
            }

            _context.SaveChanges();
            TempData["Success"] = "Subjects enrolled successfully.";
            return RedirectToAction("EnrollSubjects");
        }

        public ActionResult MyEnrolledSubjects()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            var enrollments = _context.StudentSubjectEnrollments
                .Include(e => e.Subject)
                .Include(e => e.Grade)
                .Where(e => e.StudentId == studentId)
                .ToList();

            var grouped = enrollments
                .GroupBy(e => e.Grade.Description)
                .Select(g => new EnrolledGradeViewModel
                {
                    GradeName = g.Key,
                    Subjects = g.Select(e => new EnrolledSubjectViewModel
                    {
                        SubjectCode = e.Subject.SubjectCode,
                        SubjectName = e.Subject.Name
                    }).ToList()
                }).ToList();

            return View(grouped);
        }

        private List<SelectListItem> GetGrades()
        {
            return _context.Grades
                .Where(g => g.GradeLevel >= 8 && g.GradeLevel <= 12)
                .OrderBy(g => g.GradeLevel)
                .Select(g => new SelectListItem
                {
                    Text = g.Description,
                    Value = g.GradeLevel.ToString()
                })
                .ToList();
        }

        private List<SelectListItem> GetYears()
        {
            int currentYear = DateTime.Now.Year;
            return new List<SelectListItem>
    {
        new SelectListItem { Text = currentYear.ToString(), Value = currentYear.ToString() },
        new SelectListItem { Text = (currentYear + 1).ToString(), Value = (currentYear + 1).ToString() }
    };
        }

        private List<SelectListItem> GetStudentTypes()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Text = "New Student", Value = "New" },
        new SelectListItem { Text = "Returning Student", Value = "Returning" }
    };
        }

        public JsonResult GetStreams()
        {
            var streams = _context.Streams.ToList()
                .Select(s => new
                {
                    Value = s.Id,
                    Text = s.Name
                });

            return Json(streams, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSubjectsByGrade(int gradeId, int? streamId)
        {
            var query = _context.Subjects.Where(s => s.GradeId == gradeId);

            if (streamId.HasValue)
            {
                query = query.Where(s => s.StreamId == streamId.Value);
            }

            var subjects = query.ToList()
                .Select(s => new
                {
                    Value = s.Id,
                    Text = $"{s.SubjectCode} - {s.Name}"
                });

            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReportCard()
        {
            var studentId = (int)Session["StudentId"];
            var student = _context.Students.Find(studentId);

            ViewBag.StudentName = student.FullName;
            ViewBag.StudentId = student.Id;

            var marks = _context.Marks
                .Include(m => m.Subject)
                .Include(m => m.Grade)
                .Where(m => m.StudentId == studentId)
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

        public ActionResult ExportToPdf()
        {
            var studentId = (int)Session["StudentId"];
            var student = _context.Students.Find(studentId);

            var marks = _context.Marks
                .Include(m => m.Subject)
                .Include(m => m.Grade)
                .Where(m => m.StudentId == studentId)
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

            return new Rotativa.ViewAsPdf("ReportCardPdf", grouped)
            {
                FileName = "ProgressReport.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
        }

        public ActionResult MyAttendance(DateTime? fromDate, DateTime? toDate, int? subjectId)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            var query = _context.Attendances
                .Where(a => a.StudentId == studentId);

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

            return View(model);
        }

        public ActionResult ViewMaterials(StudentMaterialFilterViewModel filter)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            var enrolledSubjectIds = _context.StudentSubjectEnrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.SubjectId)
                .ToList();

            var query = _context.LearningMaterials
                .Where(m => enrolledSubjectIds.Contains(m.SubjectId));

            if (filter.SubjectId.HasValue)
                query = query.Where(m => m.SubjectId == filter.SubjectId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(m => m.UploadedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(m => m.UploadedAt <= filter.ToDate.Value);

            var materials = query
                .OrderByDescending(m => m.UploadedAt)
                .Select(m => new StudentMaterialViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    SubjectName = m.Subject.SubjectCode + " - " + m.Subject.Name,
                    FilePath = m.FilePath,
                    UploadedAt = m.UploadedAt
                })
                .ToList();

            filter.Materials = materials;
            filter.SubjectOptions = enrolledSubjectIds
                .Select(id => _context.Subjects.Find(id))
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SubjectCode + " - " + s.Name
                });

            return View(filter);
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = _context.Students.FirstOrDefault(s => s.Email == model.Email);
            if (student == null)
            {
                TempData["Message"] = "No account found with that email.";
                return RedirectToAction("ForgotPassword");
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
            string resetLink = Url.Action("ResetPassword", "Student", new { token = token }, Request.Url.Scheme);

            // ✉️ Send email via SMTP
            try
            {
                var mail = new MailMessage();
                mail.To.Add(model.Email);
                mail.Subject = "Password Reset Request";
                mail.Body = $"Hi {student.FullName},\n\nClick the link below to reset your password:\n{resetLink}\n\nThis link will expire in 1 hour.";
                mail.IsBodyHtml = false;

                var smtp = new SmtpClient();
                smtp.Send(mail);


                smtp.Send(mail);

                TempData["Message"] = "A password reset link has been sent to your email.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to send email. Please try again later.";
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
            }

            return RedirectToAction("ForgotPassword","Student");
        }


        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Student");
            }

            var vm = new StudentResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        public ActionResult ResetPassword(StudentResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetEntry = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == model.Token && t.ExpiryDate > DateTime.Now);
            if (resetEntry == null)
            {
                TempData["Message"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword", "Student");
            }

            var student = _context.Students.FirstOrDefault(s => s.Email == resetEntry.Email);
            if (student == null)
            {
                TempData["Message"] = "Student account not found.";
                return RedirectToAction("ForgotPassword", "Student");
            }

            student.PasswordHash = (model.NewPassword);
            _context.PasswordResetTokens.Remove(resetEntry);
            _context.SaveChanges();

            TempData["Message"] = "Your password has been reset successfully.";
            return RedirectToAction("Login", "Student");
        }

        // ---------------- TUITION PAYMENT PAGE ----------------
        [HttpGet]
        public ActionResult TuitionPayment()
        {
            try
            {
                if (Session["StudentId"] == null)
                {
                    TempData["Error"] = "Please login first.";
                    return RedirectToAction("Login");
                }

                int studentId = (int)Session["StudentId"];
                var student = _context.Students.Find(studentId);

                if (student == null)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Login");
                }

                // Check if already paid
                bool hasPaid = _context.TuitionPayments
                                .Any(p => p.StudentId == studentId && p.Status == "Paid");

                if (hasPaid)
                {
                    // Redirect to success page if already paid
                    return RedirectToAction("PaymentSuccess", new { studentId = studentId });
                }

                var model = new TuitionPaymentViewModel
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    Amount = 20000.00m,
                    StripePublishableKey = ConfigurationManager.AppSettings["StripePublishableKey"],
                    HasPaid = false
                };

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TuitionPayment Page Error: {ex.Message}");
                TempData["Error"] = "Error loading payment page. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }

        // ---------------- PROCESS TUITION PAYMENT ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessTuitionPayment()
        {
            try
            {
                if (Session["StudentId"] == null)
                {
                    return Json(new { success = false, message = "Please login first." });
                }

                int studentId = (int)Session["StudentId"];
                var student = _context.Students.Find(studentId);

                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found." });
                }

                // Check if already paid
                bool hasPaid = _context.TuitionPayments
                                .Any(p => p.StudentId == studentId && p.Status == "Paid");

                if (hasPaid)
                {
                    return Json(new { success = false, message = "You have already paid your tuition fee." });
                }

                // Build absolute URLs for Stripe
                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                string successUrl = $"{baseUrl}/Student/PaymentSuccess?studentId={studentId}";
                string cancelUrl = $"{baseUrl}/Student/TuitionPayment";

                // Create Stripe session
                StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeSecretKey"];

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "zar",
                        UnitAmount = 2000000, // R20,000 in cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Tuition Fee Payment",
                            Description = $"Tuition fee for {student.FullName}"
                        }
                    },
                    Quantity = 1
                }
            },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    CustomerEmail = student.Email,
                    ClientReferenceId = studentId.ToString(),
                    Metadata = new Dictionary<string, string>
            {
                { "student_id", studentId.ToString() },
                { "student_name", student.FullName },
                { "payment_type", "tuition" }
            }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                // Create payment record
                var payment = new TuitionPayment
                {
                    StudentId = studentId,
                    PaymentDate = DateTime.Now,
                    Amount = 20000.00m,
                    Status = "Pending",
                    TransactionId = session.Id
                };

                _context.TuitionPayments.Add(payment);
                _context.SaveChanges();

                return Json(new { success = true, url = session.Url });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessTuitionPayment Error: {ex.Message}");
                return Json(new { success = false, message = "Error creating payment session. Please try again." });
            }
        }

        // ---------------- PAYMENT SUCCESS PAGE ----------------
        [HttpGet]
        public ActionResult PaymentSuccess(int studentId)
        {
            try
            {
                if (Session["StudentId"] == null || (int)Session["StudentId"] != studentId)
                {
                    TempData["Error"] = "Please login first.";
                    return RedirectToAction("Login");
                }

                var student = _context.Students.Find(studentId);
                if (student == null)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Login");
                }

                // Find the payment record
                var payment = _context.TuitionPayments
                    .Where(p => p.StudentId == studentId)
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                if (payment == null)
                {
                    TempData["Error"] = "Payment record not found.";
                    return RedirectToAction("TuitionPayment");
                }

                // If payment is still pending, verify with Stripe
                if (payment.Status == "Pending")
                {
                    StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeSecretKey"];
                    var sessionService = new SessionService();

                    try
                    {
                        var session = sessionService.Get(payment.TransactionId);
                        if (session.PaymentStatus == "paid")
                        {
                            payment.Status = "Paid";
                            payment.TransactionId = session.PaymentIntentId;
                            payment.PaymentDate = DateTime.Now;
                            _context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Stripe verification error: {ex.Message}");
                        // Continue to show success page even if verification fails
                    }
                }

                var model = new PaymentSuccessViewModel
                {
                    StudentId = studentId,
                    FullName = student.FullName,
                    Amount = payment.Amount,
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate,
                    Status = payment.Status
                };

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PaymentSuccess Error: {ex.Message}");
                TempData["Error"] = "Error loading payment confirmation.";
                return RedirectToAction("Dashboard");
            }
        }

        // ---------------- PAYMENT CANCEL ----------------
        [HttpGet]
        public ActionResult PaymentCancel()
        {
            TempData["Error"] = "Payment was cancelled. You can try again anytime.";
            return RedirectToAction("TuitionPayment");
        }

        public bool AreDocumentsApproved(int studentId)
        {
            var requiredTypes = new[] { "PreviousResults", "IDDocument" };
            var documents = _context.EnrollmentDocuments
                              .Where(d => d.StudentId == studentId && requiredTypes.Contains(d.DocumentType))
                              .ToList();

            return documents.All(d => d.Status == "Approved");
        }
        private bool HasPaidTuition(int studentId)
        {
            return _context.TuitionPayments
                .Any(p => p.StudentId == studentId && p.Status == "Paid");
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
    }
}