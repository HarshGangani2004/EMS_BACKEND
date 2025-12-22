namespace EmployeeManagement.Api.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";

        public IList<string> Roles { get; set; } = new List<string>();
    }
}
