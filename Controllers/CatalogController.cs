using ITI_FinalProject.Data;
using ITI_FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITI_FinalProject.Controllers
{
    [Authorize(Roles = "Student")]
    public class CatalogController : Controller
    {
        private const int MaxCreditHours = 18;

        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CatalogController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        private async Task<int> GetUsedHoursAsync(int studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .SumAsync(e => (int?)e.Course!.CreditHours) ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Forbid();

            var items = await _context.Courses
                .OrderBy(c => c.Code)
                .Select(c => new CourseCatalogItem
                {
                    Id = c.Id,
                    Code = c.Code,
                    Title = c.Title,
                    Instructor = c.Instructor,
                    DepartmentName = c.Department!.Name,
                    CreditHours = c.CreditHours,
                    MaximumCapacity = c.MaxCapacity,
                    Enrolled = c.Enrollments.Count(),
                    IsEnrolled = c.Enrollments.Any(e => e.StudentId == student.Id)
                })
                .ToListAsync();

            ViewBag.UsedHours = await GetUsedHoursAsync(student.Id);
            ViewBag.MaxHours = MaxCreditHours;

            return View(items);
        }

        public async Task<IActionResult> MyCourses()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Forbid();

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Department)
                .OrderBy(e => e.Course!.Code)
                .ToListAsync();

            return View(enrollments);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Forbid();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.Id && e.CourseId == courseId);
            if (alreadyEnrolled)
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(Index));
            }

            var enrolledCount = await _context.Enrollments.CountAsync(e => e.CourseId == courseId);
            if (enrolledCount >= course.MaxCapacity)
            {
                TempData["Error"] = "Sorry, this course is full.";
                return RedirectToAction(nameof(Index));
            }

            var usedHours = await GetUsedHoursAsync(student.Id);
            if (usedHours + course.CreditHours > MaxCreditHours)
            {
                TempData["Error"] = $"Credit hour limit exceeded. The maximum is {MaxCreditHours} hours and you are currently registered for {usedHours}.";
                return RedirectToAction(nameof(Index));
            }

            _context.Enrollments.Add(new Enrollment
            {
                StudentId = student.Id,
                CourseId = courseId
            });

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"You are now enrolled in {course.Title}.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Could not complete the enrollment. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Drop(int courseId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Forbid();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == courseId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "The course was dropped successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}