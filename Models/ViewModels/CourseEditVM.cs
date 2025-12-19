using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workshop1.Models.ViewModels
{
    public class CourseEditVM
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public int Credits { get; set; }
        public int Semester { get; set; }
        public string? Programme { get; set; }
        public string? EducationLevel { get; set; }

        public int? FirstTeacherId { get; set; }
        public int? SecondTeacherId { get; set; }
        public List<long> SelectedStudentIds { get; set; } = new();

        public List<SelectListItem> Teachers { get; set; } = new();
        public List<SelectListItem> AllStudents { get; set; } = new();


        public List<EnrollmentEditRowVM> EnrollmentRows { get; set; } = new();
    }

    public class EnrollmentEditRowVM
    {
        public long Id { get; set; }           
        public long StudentId { get; set; }     
        public string StudentDisplay { get; set; } = "";

        public string? Semester { get; set; }
        public int? Year { get; set; }
        public int? Grade { get; set; }

        public string? SeminarUrl { get; set; }
        public string? ProjectUrl { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminarPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        public DateTime? FinishDate { get; set; }
    }
}
