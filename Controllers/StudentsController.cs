using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITI_FinalProject.Data;
using ITI_FinalProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace ITI_FinalProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // Helper Method to Save Image
        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext) || file.Length > 2 * 1024 * 1024) return null;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ext;
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/" + fileName;
        }

        // GET: Students
        public async Task<IActionResult> Index(string? search, int? courseId, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.Students
                .Include(s => s.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s => s.FullName.Contains(term) || s.Email.Contains(term));
            }

            if (courseId.HasValue)
            {
                query = query.Where(s => _context.Enrollments
                    .Any(e => e.StudentId == s.Id && e.CourseId == courseId.Value));
            }

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Min(Math.Max(1, page), totalPages);

            var students = await query
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["CourseFilter"] = courseId;
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["CourseId"] = new SelectList(_context.Courses.OrderBy(c => c.Code), "Id", "Code", courseId);

            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Email,PhoneNumber,DateOfBirth,EnrollmentDate,DepartmentId")] Student student, IFormFile? profileImage)
        {
            if (ModelState.IsValid)
            {
                student.ProfileImagePath = await SaveImageAsync(profileImage);
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,DateOfBirth,EnrollmentDate,DepartmentId,ProfileImagePath")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
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
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // Manage Student Courses (Admin only - from class level)
        // =====================================================

        // GET: Students/ManageCourses/5
        public async Task<IActionResult> ManageCourses(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            // الكورسات المسجل فيها الطالب حاليًا
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == id)
                .OrderBy(e => e.Course!.Code)
                .ToListAsync();

            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

            // الكورسات اللي لسه مسجلش فيها
            var availableCourses = await _context.Courses
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .OrderBy(c => c.Code)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewBag.AvailableCourses = new SelectList(availableCourses, "Id", "Code");
            ViewBag.HasAvailableCourses = availableCourses.Any();

            return View(student);
        }

        // POST: Students/AddCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(int studentId, int courseId)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!studentExists || !courseExists)
            {
                return NotFound();
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "Student is already enrolled in this course.";
            }
            else
            {
                _context.Enrollments.Add(new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course added successfully.";
            }

            return RedirectToAction(nameof(ManageCourses), new { id = studentId });
        }

        // POST: Students/RemoveCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourse(int studentId, int courseId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.CourseId == courseId)
                .ToListAsync();

            if (enrollments.Any())
            {
                _context.Enrollments.RemoveRange(enrollments);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course removed successfully.";
            }
            else
            {
                TempData["Error"] = "Enrollment not found.";
            }

            return RedirectToAction(nameof(ManageCourses), new { id = studentId });
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}