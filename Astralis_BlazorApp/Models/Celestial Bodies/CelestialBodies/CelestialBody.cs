namespace Astralis_BlazorApp.Models
{
    public class CelestialBody
    {
        public int Id { get; set; }

        public int CelestialBodyTypeId { get; set; }

        public string Name { get; set; } = null!;
        
        public string? Alias { get; set; }
    }
}