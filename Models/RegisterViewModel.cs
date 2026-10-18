using System.ComponentModel.DataAnnotations;
using ITI_FinalProject.Validation;

namespace ITI_FinalProject.Models
{
    public class RegisterViewModel
    {
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
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Range(1, int.MaxValue, ErrorMessage = "Please select a department")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}