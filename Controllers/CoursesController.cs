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
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Update the constructor to accept UserManager<ApplicationUser> as a parameter
        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index(string? title, int? semester, string? programme, int? teacherId)
        {
            IQueryable<Course> query = _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher);

            // ================= ADMIN =================
            if (User.IsInRole("Admin"))
            {
                var teachers = await _context.Teachers
                    .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                    .Select(t => new
                    {
                        t.Id,
                        FullName = t.FirstName + " " + t.LastName
                    })
                    .ToListAsync();

                ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", teacherId);
            }

            // ================= TEACHER =================
            else if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user?.TeacherId == null)
                    return Forbid();

                int tid = user.TeacherId.Value;

                // ❗ КЛУЧНО: само неговите предмети
                query = query.Where(c =>
                    c.FirstTeacherId == tid ||
                    c.SecondTeacherId == tid);
            }

            // ================= STUDENT =================
            else if (User.IsInRole("Student"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.StudentId == null) return Forbid();

                var sid = (long)user.StudentId.Value;

                query = query.Where(c => _context.Enrollments
                    .Any(e => e.CourseId == c.Id && e.StudentId == sid));
            }

            var courses = await query
                .OrderBy(c => c.Semester)
                .ThenBy(c => c.Title)
                .ToListAsync();

            return View(courses);
        }


        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // optional: sort enrollments in memory (view also sorts)
            course.Enrollments = course.Enrollments
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Semester)
                .ThenBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .ToList();

            return View(course);
        }


        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            LoadTeachers();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadTeachers(course.FirstTeacherId, course.SecondTeacherId);
            return View(course);
        }

        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, int? year, string? enrollSemester)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            var y = year ?? DateTime.Now.Year;
            var sem = NormalizeSemester(enrollSemester);

            var vm = new CourseEditVM
            {
                Id = course.Id,
                Title = course.Title,
                Credits = course.Credits,
                Semester = course.Semester,
                Programme = course.Programme,
                EducationLevel = course.EducationLevel,
                FirstTeacherId = course.FirstTeacherId,
                SecondTeacherId = course.SecondTeacherId,
                EnrollYear = y,
                EnrollSemester = sem
            };

            await FillTeachers(vm);
            await FillEligibleStudents(vm);       
            await LoadCurrentEnrollments(vm);        

            // Default checked = already enrolled (for that period)
            vm.SelectedStudentIds = vm.CurrentEnrollments.Select(e => e.StudentId).ToList();

            return View(vm);
        }

        // save course fields only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, CourseEditVM vm)
        {
            if (id != vm.Id) return NotFound();

            vm.EnrollSemester = NormalizeSemester(vm.EnrollSemester);

            if (!ModelState.IsValid)
            {
                await FillTeachers(vm);
                await FillEligibleStudents(vm);
                await LoadCurrentEnrollments(vm);
                return View(vm);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            course.Title = vm.Title;
            course.Credits = vm.Credits;
            course.Semester = vm.Semester;
            course.Programme = vm.Programme;
            course.EducationLevel = vm.EducationLevel;
            course.FirstTeacherId = vm.FirstTeacherId;
            course.SecondTeacherId = vm.SecondTeacherId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = course.Id, year = vm.EnrollYear, enrollSemester = vm.EnrollSemester });
        }

        // ADMIN ONLY: enroll students for selected course+year+semester
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnrollStudents(int courseId, int enrollYear, string enrollSemester, List<long> selectedStudentIds)
        {
            enrollSemester = NormalizeSemester(enrollSemester);

            if (selectedStudentIds == null || selectedStudentIds.Count == 0)
                return RedirectToAction(nameof(Edit),
                    new { id = courseId, year = enrollYear, enrollSemester });

            // 1️⃣ Students allowed for this year/semester
            var eligibleIds = await GetEligibleStudentIds(enrollYear, enrollSemester);

            // 2️⃣ Students who ALREADY PASSED this course (Grade >= 6)
            var passedStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Grade >= 6)
                .Select(e => e.StudentId)
                .ToListAsync();

            // 3️⃣ Filter selected students:
            //    - unique
            //    - eligible by year/semester
            //    - NOT already passed the course
            var filteredSelected = selectedStudentIds
                .Distinct()
                .Where(id =>
                    eligibleIds.Contains(id) &&
                    !passedStudentIds.Contains(id))
                .ToList();

            if (filteredSelected.Count == 0)
                return RedirectToAction(nameof(Edit),
                    new { id = courseId, year = enrollYear, enrollSemester });

            // 4️⃣ Already enrolled in THIS course + year + semester
            var existingStudentIds = await _context.Enrollments
                .Where(e =>
                    e.CourseId == courseId &&
                    e.Year == enrollYear &&
                    e.Semester == enrollSemester)
                .Select(e => e.StudentId)
                .ToListAsync();

            // 5️⃣ Final list to insert (no duplicates)
            var toAdd = filteredSelected
                .Except(existingStudentIds)
                .Select(studentId => new Enrollment
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    Year = enrollYear,
                    Semester = enrollSemester,

                    Grade = null,
                    SeminarUrl = null,
                    ProjectUrl = null,
                    ExamPoints = null,
                    SeminarPoints = null,
                    ProjectPoints = null,
                    AdditionalPoints = null,
                    FinishDate = null
                })
                .ToList();

            // 6️⃣ Save safely
            if (toAdd.Count > 0)
            {
                _context.Enrollments.AddRange(toAdd);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit),
                new { id = courseId, year = enrollYear, enrollSemester });
        }

        // ADMIN ONLY: deactivate (set FinishDate) for selected enrollments
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateStudents(
            int courseId,
            int enrollYear,
            string enrollSemester,
            List<long> selectedEnrollmentIds,
            DateTime finishDateForDeactivation)
        {
            enrollSemester = NormalizeSemester(enrollSemester);

            if (selectedEnrollmentIds == null || selectedEnrollmentIds.Count == 0)
                return RedirectToAction(nameof(Edit), new { id = courseId, year = enrollYear, enrollSemester });

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId
                            && e.Year == enrollYear
                            && e.Semester == enrollSemester
                            && selectedEnrollmentIds.Contains(e.Id))
                .ToListAsync();

            foreach (var e in enrollments)
                e.FinishDate = finishDateForDeactivation;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = courseId, year = enrollYear, enrollSemester });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // -----------------------
        // Helpers
        // -----------------------

        private void LoadTeachers(int? firstTeacherId = null, int? secondTeacherId = null)
        {
            var teachers = _context.Teachers
                .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                .Select(t => new { t.Id, FullName = t.FirstName + " " + t.LastName })
                .ToList();

            ViewData["FirstTeacherId"] = new SelectList(teachers, "Id", "FullName", firstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(teachers, "Id", "FullName", secondTeacherId);
        }

        private async Task FillTeachers(CourseEditVM vm)
        {
            vm.Teachers = await _context.Teachers
                .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToListAsync();
        }

        private async Task LoadCurrentEnrollments(CourseEditVM vm)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == vm.Id)
                .OrderBy(e => e.Student.LastName)
                .ThenBy(e => e.Student.FirstName)
                .ToListAsync();

            vm.CurrentEnrollments = enrollments.Select(e => new EnrollmentRowVM
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentDisplay = $"{e.Student.FirstName} {e.Student.LastName} ({e.Student.StudentId})",
                Year = vm.EnrollYear,
                Semester = vm.EnrollSemester,
                FinishDate = e.FinishDate,
                Grade = e.Grade
            }).ToList();
        }

        // ✅ Here is the main filter: students eligible for given year+semester
        private async Task FillEligibleStudents(CourseEditVM vm)
        {
            // 1️⃣ Eligible by year + semester
            var eligibleIds = await GetEligibleStudentIds(vm.EnrollYear, vm.EnrollSemester);

            // 2️⃣ Students who have PASSED this course (Grade >= 6)
            var passedStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == vm.Id && e.Grade >= 6)
                .Select(e => e.StudentId)
                .ToListAsync();

            // 3️⃣ FINAL eligible students
            vm.AllStudents = await _context.Students
                .Where(s =>
                    eligibleIds.Contains(s.Id) &&
                    !passedStudentIds.Contains(s.Id))
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.FirstName} {s.LastName} ({s.StudentId})"
                })
                .ToListAsync();
        }


        private async Task<HashSet<long>> GetEligibleStudentIds(int year, string semester)
        {
            semester = NormalizeSemester(semester);
            var wantOdd = semester == "Winter";
            var ids = await _context.Students
                .Where(s => s.EnrollmentDate.HasValue && s.EnrollmentDate.Value.Year == year)
                .Where(s => ((s.CurrentSemester % 2) != 0) == wantOdd)
                .Select(s => s.Id)
                .ToListAsync();

            return ids.ToHashSet();
        }

        private static string NormalizeSemester(string? sem)
        {
            if (string.IsNullOrWhiteSpace(sem)) return "Winter";
            sem = sem.Trim();

            if (sem.Equals("Winter", StringComparison.OrdinalIgnoreCase)) return "Winter";
            if (sem.Equals("Summer", StringComparison.OrdinalIgnoreCase)) return "Summer";

            if (sem.Equals("Zimski", StringComparison.OrdinalIgnoreCase) || sem.Equals("Зимски", StringComparison.OrdinalIgnoreCase))
                return "Winter";
            if (sem.Equals("Leten", StringComparison.OrdinalIgnoreCase) || sem.Equals("Летен", StringComparison.OrdinalIgnoreCase))
                return "Summer";

            return "Winter";
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherView(int id, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return RedirectToAction("Index", "Home");
            var teacherId = user.TeacherId.Value;

            // провери предметот да е негов
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id &&
                    (c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId));

            if (course == null) return RedirectToAction("Index", "Home");

            int selectedYear = year ?? DateTime.Now.Year;

            // земи години што постојат за тој предмет
            var years = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Year.HasValue)
                .Select(e => e.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!years.Contains(selectedYear))
                years.Insert(0, selectedYear);

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == id && e.Year == selectedYear)
                .OrderBy(e => e.Student.StudentId)
                .ToListAsync();

            var vm = new TeacherCourseEnrollmentsVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                SelectedYear = selectedYear,
                Years = years,
                Rows = enrollments.Select(e => new TeacherEnrollmentRowVM
                {
                    EnrollmentId = e.Id,
                    StudentDbId = e.Student.Id,
                    StudentIndex = e.Student.StudentId,
                    StudentName = e.Student.FirstName + " " + e.Student.LastName,

                    IsActive = e.FinishDate == null,

                    Grade = e.Grade,
                    ExamPoints = e.ExamPoints,
                    SeminarPoints = e.SeminarPoints,
                    ProjectPoints = e.ProjectPoints,
                    AdditionalPoints = e.AdditionalPoints,
                    FinishDate = e.FinishDate,

                    SeminarUrl = e.SeminarUrl,
                    ProjectUrl = e.ProjectUrl
                }).ToList()
            };

            return View(vm);
        }

        // ✅ Save changes (само за активни студенти)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateEnrollments(TeacherCourseEnrollmentsVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.TeacherId == null) return RedirectToAction("Index", "Home");
            var teacherId = user.TeacherId.Value;

            // провери предметот да е негов
            var isMine = await _context.Courses.AnyAsync(c => c.Id == vm.CourseId &&
                (c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId));

            if (!isMine) return RedirectToAction("Index", "Home");

            var ids = vm.Rows.Select(r => r.EnrollmentId).ToList();

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == vm.CourseId && e.Year == vm.SelectedYear && ids.Contains(e.Id))
                .ToListAsync();

            foreach (var e in enrollments)
            {
                // само активни може да се менуваат
                if (e.FinishDate != null) continue;

                var row = vm.Rows.First(r => r.EnrollmentId == e.Id);

                e.ExamPoints = row.ExamPoints;
                e.SeminarPoints = row.SeminarPoints;
                e.ProjectPoints = row.ProjectPoints;
                e.AdditionalPoints = row.AdditionalPoints;

                e.Grade = row.Grade;
                e.FinishDate = row.FinishDate; // ако внесе датум => станува неактивен
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TeacherView), new { id = vm.CourseId, year = vm.SelectedYear });
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentView(int id, int? year)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null) return RedirectToAction("Index", "Home");
            var studentId = (long)user.StudentId.Value;

            int selectedYear = year ?? DateTime.Now.Year;

            // предметот мора да постои
            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return RedirectToAction("Index", "Home");

            // години за кои студентот е запишан на овој предмет
            var years = await _context.Enrollments
                .Where(e => e.CourseId == id && e.StudentId == studentId && e.Year.HasValue)
                .Select(e => e.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (years.Count == 0)
                return RedirectToAction(nameof(Index)); // не е запишан

            if (!years.Contains(selectedYear))
                selectedYear = years.First(); // default на најнова година

            // земи точно enrollment за таа година (очекуваме 1 ред)
            var e = await _context.Enrollments
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.CourseId == id && x.StudentId == studentId && x.Year == selectedYear);

            if (e == null) return RedirectToAction(nameof(Index));

            var vm = new StudentCourseEnrollmentVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                SelectedYear = selectedYear,
                Years = years,

                EnrollmentId = e.Id,
                StudentIndex = e.Student.StudentId,
                StudentName = e.Student.FirstName + " " + e.Student.LastName,

                IsActive = e.FinishDate == null,

                Grade = e.Grade,
                ExamPoints = e.ExamPoints,
                SeminarPoints = e.SeminarPoints,
                ProjectPoints = e.ProjectPoints,
                AdditionalPoints = e.AdditionalPoints,
                FinishDate = e.FinishDate,

                SeminarUrl = e.SeminarUrl, // ќе биде path до upload-нат фајл
                ProjectUrl = e.ProjectUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentEnrollment(StudentCourseEnrollmentVM vm, IFormFile? seminarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.StudentId == null) return RedirectToAction("Index", "Home");
            var studentId = (long)user.StudentId.Value;

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == vm.EnrollmentId && e.StudentId == studentId);

            if (enrollment == null) return RedirectToAction(nameof(Index));

            // студентот може да менува само свои линкови/фалјови и тоа ако сака (може и кога е finished ако сакаш - ти кажи)
            // ако сакаш да е само кога е active:
            // if (enrollment.FinishDate != null) return RedirectToAction(nameof(StudentView), new { id = vm.CourseId, year = vm.SelectedYear });

            // 1) ProjectUrl (GitHub)
            enrollment.ProjectUrl = string.IsNullOrWhiteSpace(vm.ProjectUrl) ? null : vm.ProjectUrl.Trim();

            // 2) Seminar File upload (doc/docx/pdf)
            if (seminarFile != null && seminarFile.Length > 0)
            {
                var allowed = new[] { ".doc", ".docx", ".pdf" };
                var ext = Path.GetExtension(seminarFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    TempData["Error"] = "Seminar file must be .doc, .docx or .pdf";
                    return RedirectToAction(nameof(StudentView), new { id = vm.CourseId, year = vm.SelectedYear });
                }

                // избриши стар ако постои
                if (!string.IsNullOrWhiteSpace(enrollment.SeminarUrl))
                    DeleteFileIfExists(enrollment.SeminarUrl);

                enrollment.SeminarUrl = await SaveFile(seminarFile, "seminars");
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Saved!";
            return RedirectToAction(nameof(StudentView), new { id = vm.CourseId, year = vm.SelectedYear });
        }


        private async Task<string> SaveFile(IFormFile file, string subfolder)
        {
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subfolder);
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{subfolder}/{fileName}";
        }

        private void DeleteFileIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }


    }


}

