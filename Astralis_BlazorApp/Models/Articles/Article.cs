namespace Astralis_BlazorApp.Models
{  
    public class Article
    { 
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public bool IsPremium { get; set; } = false;
    }
}
