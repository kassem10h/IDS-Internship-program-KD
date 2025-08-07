namespace Smart_Meeting.DTOs
{
    public class RoomDto
    {
        public string RoomName { get; set; } = string.Empty;
 
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; }

    }
}
