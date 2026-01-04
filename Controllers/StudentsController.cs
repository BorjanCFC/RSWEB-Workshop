using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workshop1.Data;
using Workshop1.Models;
using Workshop1.Models.ViewModels;

namespace Workshop1.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Details", new { id = user.StudentId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["Error"] = "Please select an image first.";
                return RedirectToAction(nameof(MyProfile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null)
                return RedirectToAction("Index", "Home");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.StudentId.Value);
            if (student == null)
                return RedirectToAction("Index", "Home");

            // delete old
            DeleteFileIfExists(student.ProfileImagePath);

            // save new
            student.ProfileImagePath = await SaveImage(profileImage, "students");
            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile image updated!";
            return RedirectToAction(nameof(Details), new { id = student.Id });
        }

        // GET: Students
        [Authorize(Roles = "Admin")]
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
        [Authorize]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new StudentCreateVM());

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(StudentCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 0) не дозволувај ист email
            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");
                return View(vm);
            }

            // 0.1) не дозволувај ист StudentId (ако сакаш)
            var idExists = await _context.Students.AnyAsync(s => s.StudentId == vm.StudentId);
            if (idExists)
            {
                ModelState.AddModelError(nameof(vm.StudentId), "StudentId already exists.");
                return View(vm);
            }

            // 1) креирај Student record
            var student = new Student
            {
                StudentId = vm.StudentId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                AcquiredCredits = vm.AcquiredCredits,
                EnrollmentDate = vm.EnrollmentDate,
                CurrentSemester = vm.CurrentSemester,
                EducationLevel = vm.EducationLevel
            };

            if (vm.ProfileImage != null && vm.ProfileImage.Length > 0)
            {
                student.ProfileImagePath = await SaveImage(vm.ProfileImage, "students");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // 2) креирај Identity account
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                StudentId = (int?)student.Id // must exist in ApplicationUser
            };

            var createRes = await _userManager.CreateAsync(user, vm.Password);
            if (!createRes.Succeeded)
            {
                // rollback student ако падне user
                DeleteFileIfExists(student.ProfileImagePath);
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                foreach (var err in createRes.Errors)
                    ModelState.AddModelError("", err.Description);

                return View(vm);
            }

            // 3) додели role Student
            await _userManager.AddToRoleAsync(user, "Student");

            // 4) линк Student -> ApplicationUserId (ако го имаш во Student)
            student.ApplicationUserId = user.Id;
            _context.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long id, Student student)
        {
            if (id != student.Id) return NotFound();

            var dbStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (dbStudent == null) return NotFound();

            // задржи што не сакаш да се изгуби
            student.ProfileImagePath = dbStudent.ProfileImagePath;
            student.ApplicationUserId = dbStudent.ApplicationUserId;

            if (student.ProfileImage != null && student.ProfileImage.Length > 0)
            {
                DeleteFileIfExists(dbStudent.ProfileImagePath);
                student.ProfileImagePath = await SaveImage(student.ProfileImage, "students");
            }

            if (!ModelState.IsValid)
                return View(student);

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

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return RedirectToAction(nameof(Index));

            // 1) избриши слика
            DeleteFileIfExists(student.ProfileImagePath);

            // 2) избриши identity user ако постои
            if (!string.IsNullOrWhiteSpace(student.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Count > 0)
                        await _userManager.RemoveFromRolesAsync(user, roles);

                    await _userManager.DeleteAsync(user);
                }
            }
            else
            {
                // fallback преку StudentId во ApplicationUser
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.StudentId == student.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Count > 0)
                        await _userManager.RemoveFromRolesAsync(user, roles);

                    await _userManager.DeleteAsync(user);
                }
            }

            // 3) избриши student
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

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
