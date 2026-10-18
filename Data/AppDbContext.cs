using ITI_FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ITI_FinalProject.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>()
                .HasIndex(department => department.Name)
                .IsUnique();
            modelBuilder.Entity<Student>()
                .HasIndex(student => student.Email)
                .IsUnique();
            modelBuilder.Entity<Course>()
                .HasIndex(course => course.Code)
                .IsUnique();
            modelBuilder.Entity<Enrollment>()
                .HasIndex(enrollment => new { enrollment.StudentId, enrollment.CourseId })
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(student => student.Department)
                .WithMany(department => department.Students)
                .HasForeignKey(student => student.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Course>()
                .HasOne(course => course.Department)
                .WithMany(department => department.Courses)
                .HasForeignKey(course => course.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Enrollment>()
                .HasOne(enrollment => enrollment.Student)
                .WithMany(student => student.Enrollments)
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Enrollment>()
                .HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}