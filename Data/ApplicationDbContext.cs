using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using Workshop1.Models;

namespace Workshop1.Data
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentId)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasOne(c => c.FirstTeacher)
                .WithMany(t => t.FirstTeacherCourses)
                .HasForeignKey(c => c.FirstTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.SecondTeacher)
                .WithMany(t => t.SecondTeacherCourses)
                .HasForeignKey(c => c.SecondTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);

            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.CourseId, e.StudentId })
                .IsUnique();

            // ======================
            // TEACHERS
            // ======================
            modelBuilder.Entity<Teacher>().HasData(
                new Teacher { Id = 1, FirstName = "Ivan", LastName = "Petrovski", Degree = "PhD", AcademicRank = "Professor", OfficeNumber = "A101" },
                new Teacher { Id = 2, FirstName = "Ana", LastName = "Stojanova", Degree = "MSc", AcademicRank = "Assistant", OfficeNumber = "B202" },
                new Teacher { Id = 3, FirstName = "Marko", LastName = "Iliev", Degree = "PhD", AcademicRank = "Associate Professor", OfficeNumber = "A203" },
                new Teacher { Id = 4, FirstName = "Elena", LastName = "Kostova", Degree = "MSc", AcademicRank = "Lecturer", OfficeNumber = "C104" },
                new Teacher { Id = 5, FirstName = "Stefan", LastName = "Dimitrov", Degree = "PhD", AcademicRank = "Professor", OfficeNumber = "A105" }
            );

            // ======================
            // COURSES
            // ======================
            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Title = "Databases", Credits = 6, Semester = 3, Programme = "IT", EducationLevel = "Undergraduate", FirstTeacherId = 1 },
                new Course { Id = 2, Title = "Web Programming", Credits = 6, Semester = 4, Programme = "IT", EducationLevel = "Undergraduate", FirstTeacherId = 2 },
                new Course { Id = 3, Title = "Software Engineering", Credits = 7, Semester = 5, Programme = "SE", EducationLevel = "Undergraduate", FirstTeacherId = 3 },
                new Course { Id = 4, Title = "Computer Networks", Credits = 6, Semester = 4, Programme = "IT", EducationLevel = "Undergraduate", FirstTeacherId = 4 },
                new Course { Id = 5, Title = "Artificial Intelligence", Credits = 7, Semester = 6, Programme = "SE", EducationLevel = "Undergraduate", FirstTeacherId = 5 }
            );

            // ======================
            // STUDENTS
            // ======================
            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, StudentId = "201001", FirstName = "Petar", LastName = "Nikolov", EnrollmentDate = new DateTime(2022, 10, 1), CurrentSemester = 3 },
                new Student { Id = 2, StudentId = "201002", FirstName = "Marija", LastName = "Stankova", EnrollmentDate = new DateTime(2022, 10, 1), CurrentSemester = 3 },
                new Student { Id = 3, StudentId = "201003", FirstName = "Jovan", LastName = "Trajkov", EnrollmentDate = new DateTime(2021, 10, 1), CurrentSemester = 5 },
                new Student { Id = 4, StudentId = "201004", FirstName = "Sara", LastName = "Mihajlova", EnrollmentDate = new DateTime(2021, 10, 1), CurrentSemester = 5 },
                new Student { Id = 5, StudentId = "201005", FirstName = "David", LastName = "Kirilov", EnrollmentDate = new DateTime(2020, 10, 1), CurrentSemester = 6 }
            );

            // ======================
            // ENROLLMENTS
            // ======================
            modelBuilder.Entity<Enrollment>().HasData(
                new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Year = 2023, Semester = "Winter", Grade = 8 },
                new Enrollment { Id = 2, StudentId = 1, CourseId = 2, Year = 2023, Semester = "Winter", Grade = 9 },
                new Enrollment { Id = 3, StudentId = 2, CourseId = 1, Year = 2023, Semester = "Winter", Grade = 7 },
                new Enrollment { Id = 4, StudentId = 3, CourseId = 3, Year = 2024, Semester = "Summer", Grade = 10 },
                new Enrollment { Id = 5, StudentId = 4, CourseId = 4, Year = 2024, Semester = "Summer", Grade = 8 }
            );
        }
    }
}
