using System;
using System.ComponentModel.DataAnnotations;


namespace Smart_Meeting.Models
{
    public class Room
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please write room name…")]
        public string RoomName { get; set; } = string.Empty;
        [Required]
        public int Capacity { get; set; }
        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public required RoomFeatures RoomFeatures { get; set; }
    }
}
