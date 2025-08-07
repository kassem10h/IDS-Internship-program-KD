using System.ComponentModel.DataAnnotations;

namespace Smart_Meeting.Models
{
    public class RoomFeatures
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public bool HasProjector { get; set; }
        [Required]
        public bool HasWhiteBoard { get; set; }
        [Required]
        public bool HasVideoConferencing { get; set; }
        [Required]
        public int RoomID { get; set; }

        [Required]
        public required Room Room { get; set; }
    }
}
