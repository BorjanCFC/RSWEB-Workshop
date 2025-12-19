using System.ComponentModel.DataAnnotations;

namespace Workshop1.Models
{
    public class Enrollment
    {
        [Key]
        public long Id { get; set; }

        public int CourseId { get; set; }
        public long StudentId { get; set; }

        [StringLength(10)]
        public string? Semester { get; set; }

        public int? Year { get; set; }

        public int? Grade { get; set; }

        [StringLength(255)]
        public string? SeminarUrl { get; set; }

        [StringLength(255)]
        public string? ProjectUrl { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public DateTime? FinishDate { get; set; }

        // Navigation
        public Course Course { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
