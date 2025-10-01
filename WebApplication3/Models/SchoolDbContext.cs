using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using WebApplication23.Models;

namespace WebApplication3.Models
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext() : base("DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<PreviousResult> PreviousResults { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Stream> Streams { get; set; }
        public DbSet<StreamSubject> StreamSubjects { get; set; }
        public DbSet<EnrollmentDocument> EnrollmentDocuments { get; set; }
        public DbSet<EnrollmentSubject> EnrollmentSubjects { get; set; }
        public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments { get; set; }
        public DbSet<StudentSubjectEnrollment> StudentSubjectEnrollments { get; set; }
        public DbSet<Mark> Marks { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<LearningMaterial> LearningMaterials { get; set; }
        public DbSet<ClassTimetable> ClassTimetables { get; set; }
        public DbSet<TuitionPayment> TuitionPayments { get; set; }
        // NEW: Uniform Shop
        public DbSet<UniformCategory> UniformCategories { get; set; }
        public DbSet<UniformItem> UniformItems { get; set; }
        public DbSet<UniformOrder> UniformOrders { get; set; }
        public DbSet<UniformOrderItem> UniformOrderItems { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .HasOptional(s => s.Stream)
                .WithMany(st => st.Subjects)
                .HasForeignKey(s => s.StreamId)
                .WillCascadeOnDelete(false); // 👈 Prevents multiple cascade paths
        }


    }
}