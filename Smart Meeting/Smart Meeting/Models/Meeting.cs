using Smart_Meeting.Models;
using System.ComponentModel.DataAnnotations;

namespace Smart_Meeting.Models
{
    public class Meeting
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please write room name…")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write room agenda…")]
        public string Agenda { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int RoomID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        public Room? Room { get; set; }
        public Employee? Employee { get; set; }

        public MinutesOfMeeting? MinutesOfMeeting { get; set; }
        public List<Attendee> Attendees { get; set; } = new();
    }
}
