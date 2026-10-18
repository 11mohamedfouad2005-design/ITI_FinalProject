using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace ITI_FinalProject.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "Enrollment date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [Precision(5, 2)]
        public decimal Grade { get; set; }
    }
}
