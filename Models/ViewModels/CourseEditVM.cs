using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workshop1.Models.ViewModels
{
    public class CourseEditVM
    {
        // -----------------------
        // Course fields
        // -----------------------
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Credits { get; set; }
        public int Semester { get; set; }
        public string? Programme { get; set; }
        public string? EducationLevel { get; set; }

        public int? FirstTeacherId { get; set; }
        public int? SecondTeacherId { get; set; }

        public List<SelectListItem> Teachers { get; set; } = new();

        // -----------------------
        // Phase 2 (Admin enroll/unenroll)
        // -----------------------
        public int EnrollYear { get; set; } = DateTime.Now.Year; // admin enters
        public string EnrollSemester { get; set; } = "Winter";   // Winter/Summer

        // filtered list of eligible students for that period
        public List<SelectListItem> AllStudents { get; set; } = new();

        // selected students to enroll
        public List<long> SelectedStudentIds { get; set; } = new();

        // current enrollments for selected period
        public List<EnrollmentRowVM> CurrentEnrollments { get; set; } = new();

        // selected enrollments to deactivate
        public List<long> SelectedEnrollmentIds { get; set; } = new();

        // finish date used for deactivation
        public DateTime? FinishDateForDeactivation { get; set; }
    }

    public class EnrollmentRowVM
    {
        public long Id { get; set; }
        public long StudentId { get; set; }
        public string StudentDisplay { get; set; } = "";

        public int Year { get; set; }
        public string Semester { get; set; } = "Winter";

        public DateTime? FinishDate { get; set; }

        public int? Grade { get; set; }
    }
}
