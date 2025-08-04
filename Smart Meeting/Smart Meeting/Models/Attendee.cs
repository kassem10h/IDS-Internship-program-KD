using System.ComponentModel.DataAnnotations;

namespace Smart_Meeting.Models
{
    public class Attendee
    {
        [Key]
        public int ID { get; set; }

        public int EmployeeID { get; set; }

        public int MeetingID { get; set; }

        public Employee? employee { get; set; }

        public Meeting? meeting { get; set; }
    }
}
