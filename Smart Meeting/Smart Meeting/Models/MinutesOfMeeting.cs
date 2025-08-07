using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Meeting.Models
{
    public class MinutesOfMeeting
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public string Decisions { get; set; } = string.Empty;
        [Required]
        public bool Status { get; set;}

        public int MeetingID { get; set; }
        public Meeting? Meeting { get; set; }

        public int AuthorId { get; set; }
        public Employee? Author { get; set; }

    }
}
