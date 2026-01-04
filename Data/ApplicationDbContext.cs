using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Workshop1.Models;

namespace Workshop1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
                .HasIndex(e => new { e.CourseId, e.StudentId })
                .IsUnique();

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);
        }
    }
}
