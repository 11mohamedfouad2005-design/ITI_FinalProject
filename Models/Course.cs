using System.ComponentModel.DataAnnotations;

namespace ITI_FinalProject.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Range(1, 6)]
        [Display(Name = "Credit hours")]
        public int CreditHours { get; set; }

        [Required, Range(1, 500)]
        [Display(Name = "Maximum capacity")]
        public int MaxCapacity { get; set; }

        [Required, StringLength(100)]
        public string Instructor { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}