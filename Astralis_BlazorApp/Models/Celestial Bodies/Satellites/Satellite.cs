namespace Astralis_BlazorApp.Models
{
    public class Satellite
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int PlanetId { get; set; }

        public decimal? Gravity { get; set; }

        public decimal? Radius { get; set; }

        public decimal? Density { get; set; }
    }
}