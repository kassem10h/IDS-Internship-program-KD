using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace
        Smart_Meeting.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public string? Role { get; set; } = "Employee";

        [Column(TypeName = "nvarchar(50)")]
        public required string FirstName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public required string LastName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public required string Email { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public required string Password { get; set; }
    }
}
