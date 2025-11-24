namespace Astralis_BlazorApp.Models
{    
    public class Event
    {  
        public int Id { get; set; }

        public int EventTypeId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Location { get; set; }

        public string? Link { get; set; }
    }
}
