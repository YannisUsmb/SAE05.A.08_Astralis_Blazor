namespace Astralis_BlazorApp.Models
{ 
    public class Comment
    {    
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ArticleId { get; set; }

        public int? RepliesToId { get; set; }

        public string Text { get; set; } = null!;
  
        public DateTime Date { get; set; } = DateTime.UtcNow;
   
        public bool IsVisible { get; set; } = true;
    }
}
