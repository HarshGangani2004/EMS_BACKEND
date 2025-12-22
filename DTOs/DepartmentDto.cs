namespace EmployeeManagement.Api.DTOs
{
    public class DepartmentDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? CreatedBy { get; set; }

    }

}
