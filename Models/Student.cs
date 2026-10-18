using System.ComponentModel.DataAnnotations;
using ITI_FinalProject.Validation;

namespace ITI_FinalProject.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(30)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, DataType(DataType.Date), MinimumStudentAge(16)]
        [Display(Name = "Date of birth")]
        public DateTime DateOfBirth { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Enrollment date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Today;

        [Range(1, int.MaxValue)]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? UserId { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}