namespace Astralis_BlazorApp.Models
{
    public class City
    {
        public int Id { get; set; }

        public int CountryId { get; set; }

        public string Name { get; set; } = null!;

        public string PostCode { get; set; } = null!;
 
        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
