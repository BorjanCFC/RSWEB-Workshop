using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Workshop1.Models.ViewModels
{
    public class TeacherCreateVM
    {
        // Teacher fields
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        public string? Degree { get; set; }
        public string? AcademicRank { get; set; }
        public string? OfficeNumber { get; set; }
        public DateTime? HireDate { get; set; }

        // Account fields
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, MinLength(6)] public string Password { get; set; } = null!;
        public IFormFile? ProfileImage { get; set; } // <-- Add this property to fix CS1061
    }
}
