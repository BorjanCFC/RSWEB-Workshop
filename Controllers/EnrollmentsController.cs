using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workshop1.Data;
using Workshop1.Models;

namespace Workshop1.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .OrderByDescending(e => e.Year)
                .ThenBy(e => e.Semester)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(enrollment.CourseId, enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            LoadDropdowns(enrollment.CourseId, enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Enrollment enrollment)
        {
            if (id != enrollment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Enrollments.Any(e => e.Id == enrollment.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(enrollment.CourseId, enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? courseId = null, long? studentId = null)
        {
            // Courses dropdown
            ViewData["CourseId"] = new SelectList(
                _context.Courses
                    .OrderBy(c => c.Title)
                    .Select(c => new { c.Id, c.Title })
                    .ToList(),
                "Id",
                "Title",
                courseId
            );

            // Students dropdown (FullName + StudentId)
            ViewData["StudentId"] = new SelectList(
                _context.Students
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Select(s => new
                    {
                        s.Id,
                        FullName = s.FirstName + " " + s.LastName + " (" + s.StudentId + ")"
                    })
                    .ToList(),
                "Id",
                "FullName",
                studentId
            );
        }
    }
}
