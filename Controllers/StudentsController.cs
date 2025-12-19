using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workshop1.Data;
using Workshop1.Models;

namespace Workshop1.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? index, string? firstName, string? lastName, int? courseId)
        {
            ViewBag.Courses = new SelectList(
                await _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new { c.Id, c.Title })
                    .ToListAsync(),
                "Id",
                "Title",
                courseId
            );

            IQueryable<Student> query = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(index))
                query = query.Where(s => s.StudentId.Contains(index));

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(s => s.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(s => s.LastName.Contains(lastName));

            if (courseId.HasValue)
                query = query.Where(s => s.Enrollments.Any(e => e.CourseId == courseId.Value));

            var students = await query
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create() => View();

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (student.ProfileImage != null && student.ProfileImage.Length > 0)
            {
                student.ProfileImagePath = await SaveImage(student.ProfileImage, "students");
            }

            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Student student)
        {
            if (id != student.Id) return NotFound();

            var dbStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (dbStudent == null) return NotFound();

            student.ProfileImagePath = dbStudent.ProfileImagePath;

            if (student.ProfileImage != null && student.ProfileImage.Length > 0)
            {
                DeleteFileIfExists(dbStudent.ProfileImagePath);

                student.ProfileImagePath = await SaveImage(student.ProfileImage, "students");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                DeleteFileIfExists(student.ProfileImagePath);
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id) => _context.Students.Any(e => e.Id == id);

        private async Task<string> SaveImage(IFormFile file, string subfolder)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Only image files (.jpg, .jpeg, .png, .webp) are allowed.");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // return relative path stored in DB
            return $"/uploads/{subfolder}/{fileName}";
        }

        private void DeleteFileIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
