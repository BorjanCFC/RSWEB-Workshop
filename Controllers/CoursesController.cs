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

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string? title, int? semester, string? programme, int? teacherId)
        {
            ViewBag.Teachers = new SelectList(
                await _context.Teachers
                    .OrderBy(t => t.LastName)
                    .ThenBy(t => t.FirstName)
                    .Select(t => new
                    {
                        t.Id,
                        FullName = t.FirstName + " " + t.LastName
                    })
                    .ToListAsync(),
                "Id",
                "FullName",
                teacherId
            );

            IQueryable<Course> query = _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(c => c.Title.Contains(title));

            if (semester.HasValue)
                query = query.Where(c => c.Semester == semester.Value);

            if (!string.IsNullOrWhiteSpace(programme))
                query = query.Where(c => c.Programme != null && c.Programme.Contains(programme));

            if (teacherId.HasValue)
            {
                query = query.Where(c =>
                    (c.FirstTeacherId.HasValue && c.FirstTeacherId == teacherId) ||
                    (c.SecondTeacherId.HasValue && c.SecondTeacherId == teacherId));
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
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            LoadTeachers();
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

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

                SelectedStudentIds = course.Enrollments.Select(e => e.StudentId).ToList(),

                EnrollmentRows = course.Enrollments
                    .OrderBy(e => e.Student.LastName)
                    .ThenBy(e => e.Student.FirstName)
                    .Select(e => new EnrollmentEditRowVM
                    {
                        Id = e.Id,
                        StudentId = e.StudentId,
                        StudentDisplay = e.Student.FirstName + " " + e.Student.LastName + " (" + e.Student.StudentId + ")",

                        Semester = e.Semester,
                        Year = e.Year,
                        Grade = e.Grade,
                        SeminarUrl = e.SeminarUrl,
                        ProjectUrl = e.ProjectUrl,
                        ExamPoints = e.ExamPoints,
                        SeminarPoints = e.SeminarPoints,
                        ProjectPoints = e.ProjectPoints,
                        AdditionalPoints = e.AdditionalPoints,
                        FinishDate = e.FinishDate
                    })
                    .ToList()
            };

            await FillCourseEditLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseEditVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await FillCourseEditLists(vm);
                return View(vm);
            }

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // 1) Update course fields
            course.Title = vm.Title;
            course.Credits = vm.Credits;
            course.Semester = vm.Semester;
            course.Programme = vm.Programme;
            course.EducationLevel = vm.EducationLevel;
            course.FirstTeacherId = vm.FirstTeacherId;
            course.SecondTeacherId = vm.SecondTeacherId;

            // 2) Update enroll/un-enroll via checkbox list
            var selected = (vm.SelectedStudentIds ?? new List<long>()).ToHashSet();
            var existing = course.Enrollments.Select(e => e.StudentId).ToHashSet();

            // Remove enrollments that are unchecked
            var toRemove = course.Enrollments.Where(e => !selected.Contains(e.StudentId)).ToList();
            _context.Enrollments.RemoveRange(toRemove);

            // Add enrollments that are newly checked
            var toAdd = selected.Except(existing);
            foreach (var studentId in toAdd)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    CourseId = course.Id,
                    StudentId = studentId,
                    Year = DateTime.Now.Year,
                    Semester = "Winter"
                });
            }

            await _context.SaveChangesAsync();

            // 3) Update other fields in Enrollment (NOT FK)
            var rows = vm.EnrollmentRows ?? new List<EnrollmentEditRowVM>();
            var rowsByStudentId = rows.ToDictionary(r => r.StudentId, r => r);

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == course.Id && selected.Contains(e.StudentId))
                .ToListAsync();

            foreach (var e in enrollments)
            {
                if (!rowsByStudentId.TryGetValue(e.StudentId, out var row))
                    continue;

                // DO NOT change CourseId / StudentId
                e.Semester = row.Semester;
                e.Year = row.Year;
                e.Grade = row.Grade;
                e.SeminarUrl = row.SeminarUrl;
                e.ProjectUrl = row.ProjectUrl;
                e.ExamPoints = row.ExamPoints;
                e.SeminarPoints = row.SeminarPoints;
                e.ProjectPoints = row.ProjectPoints;
                e.AdditionalPoints = row.AdditionalPoints;
                e.FinishDate = row.FinishDate;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Delete/5
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

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        private void LoadTeachers(int? firstTeacherId = null, int? secondTeacherId = null)
        {
            var teachers = _context.Teachers
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new
                {
                    t.Id,
                    FullName = t.FirstName + " " + t.LastName
                })
                .ToList();

            ViewData["FirstTeacherId"] = new SelectList(teachers, "Id", "FullName", firstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(teachers, "Id", "FullName", secondTeacherId);
        }

        private async Task FillCourseEditLists(CourseEditVM vm)
        {
            vm.Teachers = await _context.Teachers
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.FirstName + " " + t.LastName
                })
                .ToListAsync();

            vm.AllStudents = await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")"
                })
                .ToListAsync();
        }
    }
}
