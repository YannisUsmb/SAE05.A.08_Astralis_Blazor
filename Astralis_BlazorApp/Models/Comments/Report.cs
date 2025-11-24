namespace Astralis_BlazorApp.Models
{
    public class Report
    {
        public int Id { get; set; }

        public int ReportStatusId { get; set; }

        public int ReportMotiveId { get; set; }

        public int CommentId { get; set; }

        public int UserId { get; set; }

        public int? AdminId { get; set; }

        public string Description { get; set; } = null!;

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}