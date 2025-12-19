using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workshop1.Data;
using Workshop1.Models;

namespace Workshop1.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TeachersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string? firstName, string? lastName, string? degree, string? academicRank)
        {
            IQueryable<Teacher> query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(t => t.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(t => t.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(degree))
                query = query.Where(t => t.Degree != null && t.Degree.Contains(degree));

            if (!string.IsNullOrWhiteSpace(academicRank))
                query = query.Where(t => t.AcademicRank != null && t.AcademicRank.Contains(academicRank));

            var teachers = await query
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();

            return View(teachers);
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create() => View();

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher)
        {
            if (teacher.ProfileImage != null && teacher.ProfileImage.Length > 0)
            {
                teacher.ProfileImagePath = await SaveImage(teacher.ProfileImage, "teachers");
            }

            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher teacher)
        {
            if (id != teacher.Id) return NotFound();

            var dbTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (dbTeacher == null) return NotFound();

            teacher.ProfileImagePath = dbTeacher.ProfileImagePath;

            if (teacher.ProfileImage != null && teacher.ProfileImage.Length > 0)
            {
                DeleteFileIfExists(dbTeacher.ProfileImagePath);
                teacher.ProfileImagePath = await SaveImage(teacher.ProfileImage, "teachers");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                DeleteFileIfExists(teacher.ProfileImagePath);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id) => _context.Teachers.Any(e => e.Id == id);

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
