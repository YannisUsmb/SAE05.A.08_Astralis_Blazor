namespace Astralis_BlazorApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string Label { get; set; } = null!;

        public string? Description { get; set; }
    }
}
