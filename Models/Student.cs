using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workshop1.Models
{
    public class Student
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string StudentId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime? EnrollmentDate { get; set; }

        public int? AcquiredCredits { get; set; }

        public int? CurrentSemester { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";


        public string? ProfileImagePath { get; set; }

        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

        // Navigation
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
