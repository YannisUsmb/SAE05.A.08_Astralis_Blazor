namespace Astralis_BlazorApp.Models
{  
    public class Product
    { 
        public int Id { get; set; }
    
        public int ProductCategoryId { get; set; }

        public int UserId { get; set; }

        public string Label { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }
    }
}
