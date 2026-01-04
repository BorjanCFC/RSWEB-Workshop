using System.ComponentModel.DataAnnotations;

namespace Workshop1.Models.ViewModels
{
    public class TeacherCourseEnrollmentsVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        public int SelectedYear { get; set; }
        public List<int> Years { get; set; } = new();

        public List<TeacherEnrollmentRowVM> Rows { get; set; } = new();
    }

    public class TeacherEnrollmentRowVM
    {
        public long EnrollmentId { get; set; }

        public long StudentDbId { get; set; }
        public string StudentIndex { get; set; } = "";
        public string StudentName { get; set; } = "";

        public bool IsActive { get; set; } // FinishDate == null

        public int? Grade { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public DateTime? FinishDate { get; set; }

        public string? SeminarUrl { get; set; }
        public string? ProjectUrl { get; set; }
    }
}
