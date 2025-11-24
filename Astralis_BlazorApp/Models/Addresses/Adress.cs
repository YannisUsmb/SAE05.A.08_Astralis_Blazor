namespace Astralis_BlazorApp.Models
{    
    public class Address
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        public string StreetNumber { get; set; } = null!;
  
        public string StreetAddress { get; set; } = null!;
    }
}
