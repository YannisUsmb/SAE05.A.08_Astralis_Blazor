namespace Astralis_BlazorApp.Models
{    
    public class Command
    { 
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CommandStatusId { get; set; }
 
        public DateTime Date { get; set; }

        public decimal Total { get; set; }

        public string PdfName { get; set; } = null!;
  
        public string PdfPath { get; set; } = null!;
    }
}
