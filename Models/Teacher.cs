using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workshop1.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [StringLength(50)]
        public string? Degree { get; set; }

        [StringLength(25)]
        public string? AcademicRank { get; set; }

        [StringLength(10)]
        public string? OfficeNumber { get; set; }

        public DateTime? HireDate { get; set; }

        // Image
        public string? ProfileImagePath { get; set; }

        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation
        public ICollection<Course> FirstTeacherCourses { get; set; } = new List<Course>();
        public ICollection<Course> SecondTeacherCourses { get; set; } = new List<Course>();
    }
}
