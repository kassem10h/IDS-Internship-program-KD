using Smart_Meeting.Models;
using System.ComponentModel.DataAnnotations;

namespace Smart_Meeting.DTOs
{
    public class MeetingDto
    {
        public string Title { get; set; } = string.Empty;
        public string Agenda { get; set; } = string.Empty;


        public DateTime Date { get; set; }

        public int RoomID { get; set; }

        public int EmployeeID { get; set; }

        public List<Attendee> Attendees { get; set; } = new();
    }
}
