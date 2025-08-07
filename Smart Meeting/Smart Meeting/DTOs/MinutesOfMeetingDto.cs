using AutoMapper;
using System.ComponentModel.DataAnnotations;


namespace Smart_Meeting.DTOs
{
    public class MinutesOfMeetingDto
    {
        public string Summary { get; set; } = string.Empty;

        public string Decisions { get; set; } = string.Empty;

        public bool Status { get; set; }

        public int AuthorId { get; set; }
    }
}
