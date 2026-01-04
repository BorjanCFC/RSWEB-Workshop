using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Workshop1.Models.ViewModels
{
    public class StudentCreateVM
    {
        // Student fields
        [Required] public string StudentId { get; set; } = null!;
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        public DateTime? EnrollmentDate { get; set; }
        public int? CurrentSemester { get; set; }

        public int? AcquiredCredits { get; set; }
        public string? EducationLevel { get; set; }

        public IFormFile? ProfileImage { get; set; }

        // Account fields
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, MinLength(6)] public string Password { get; set; } = null!;
    }
}
