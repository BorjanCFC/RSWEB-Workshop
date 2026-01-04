using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workshop1.Data;
using Workshop1.Models;
using Workshop1.Models.ViewModels;

namespace Workshop1.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeachersController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Details", new { id = user.TeacherId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["Error"] = "Please select an image first.";
                return RedirectToAction(nameof(MyProfile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null)
                return RedirectToAction("Index", "Home");

            var teacher = await _context.Teachers.FirstOrDefaultAsync(s => s.Id == user.TeacherId.Value);
            if (teacher == null)
                return RedirectToAction("Index", "Home");

            // delete old
            DeleteFileIfExists(teacher.ProfileImagePath);

            // save new
            teacher.ProfileImagePath = await SaveImage(profileImage, "teachers");
            _context.Update(teacher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile image updated!";
            return RedirectToAction(nameof(Details), new { id = teacher.Id });
        }

        // GET: Teachers
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Ако твојот Create view е сеуште @model Teacher, треба да го смениш во TeacherCreateVM
            return View(new TeacherCreateVM());
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TeacherCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 0) Не дозволувај двајца users со ист email
            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");
                return View(vm);
            }

            // 1) Креирај Teacher
            var teacher = new Teacher
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Degree = vm.Degree,
                AcademicRank = vm.AcademicRank,
                OfficeNumber = vm.OfficeNumber,
                HireDate = vm.HireDate
            };

            if (vm.ProfileImage != null && vm.ProfileImage.Length > 0)
            {
                teacher.ProfileImagePath = await SaveImage(vm.ProfileImage, "teachers");
            }

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            // 2) Креирај Identity account за teacher
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                TeacherId = teacher.Id // <-- ова мора да постои во ApplicationUser
            };

            var createRes = await _userManager.CreateAsync(user, vm.Password);
            if (!createRes.Succeeded)
            {
                // Ако падне креирање user, избриши го teacher што штотуку го креиравме (rollback)
                DeleteFileIfExists(teacher.ProfileImagePath);
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                foreach (var err in createRes.Errors)
                    ModelState.AddModelError("", err.Description);

                return View(vm);
            }

            // 3) Додели role Teacher
            await _userManager.AddToRoleAsync(user, "Teacher");

            // 4) Линк Teacher -> ApplicationUser
            teacher.ApplicationUserId = user.Id; // <-- ова мора да постои во Teacher
            _context.Update(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Teachers/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Teacher teacher)
        {
            if (id != teacher.Id) return NotFound();

            var dbTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (dbTeacher == null) return NotFound();

            // Зачувај линкови што не се во формата/или не сакаш да се губат
            teacher.ProfileImagePath = dbTeacher.ProfileImagePath;
            teacher.ApplicationUserId = dbTeacher.ApplicationUserId;

            if (teacher.ProfileImage != null && teacher.ProfileImage.Length > 0)
            {
                DeleteFileIfExists(dbTeacher.ProfileImagePath);
                teacher.ProfileImagePath = await SaveImage(teacher.ProfileImage, "teachers");
            }

            if (!ModelState.IsValid)
                return View(teacher);

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

        // GET: Teachers/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return RedirectToAction(nameof(Index));

            var courses = await _context.Courses
                .Where(c => c.FirstTeacherId == id || c.SecondTeacherId == id)
                .ToListAsync();

            foreach (var c in courses)
            {
                if (c.FirstTeacherId == id) c.FirstTeacherId = null;
                if (c.SecondTeacherId == id) c.SecondTeacherId = null;
            }

            await _context.SaveChangesAsync();

            // 1) избриши слика
            DeleteFileIfExists(teacher.ProfileImagePath);

            // 2) избриши identity account ако постои
            if (!string.IsNullOrWhiteSpace(teacher.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(teacher.ApplicationUserId);
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
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TeacherId == teacher.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Count > 0)
                        await _userManager.RemoveFromRolesAsync(user, roles);

                    await _userManager.DeleteAsync(user);
                }
            }

            // 3) избриши teacher record
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

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
