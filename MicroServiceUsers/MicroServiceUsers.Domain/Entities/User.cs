namespace ServiceUser.Domain.Entities
{
    public class User : Person
    {
        public DateTime? HireDate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public string? Specialization { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool MustChangePassword { get; set; }
        public string? Role { get; set; } //instructor o admin
    }
}