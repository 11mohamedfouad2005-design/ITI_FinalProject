namespace ITI_FinalProject.Models
{
    public class CourseCatalogItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Instructor { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public int MaximumCapacity { get; set; }
        public int Enrolled { get; set; }
        public bool IsEnrolled { get; set; }
    }
}