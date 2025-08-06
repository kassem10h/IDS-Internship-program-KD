using Smart_Meeting.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Smart_Meeting.Models
{
    public class Employee : IdentityUser
    {
        [Key]
        public int EmployeeId { get; set; }

        public string? Role { get; set; } = "Employee";

        [Column(TypeName = "nvarchar(50)")]
        public required string FirstName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public required string LastName { get; set; }

        /*[Column(TypeName = "nvarchar(50)")]
        public required string Email { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public required string Password { get; set; }
        */
        public List<Attendee> Attendees { get; set; } = new();
        public List<MinutesOfMeeting> AuthoredMinutes { get; set; } = new();
    }
}