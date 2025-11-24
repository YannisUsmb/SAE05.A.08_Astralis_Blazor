namespace Astralis_BlazorApp.Models
{
    public class Planet
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int PlanetTypeId { get; set; }

        public int DetectionMethodId { get; set; }

        public string? Distance { get; set; }

        public int? DiscoveryYear { get; set; }

        public string? Mass { get; set; }

        public string? Radius { get; set; }

        public string? Temperature { get; set; }

        public decimal? OrbitalPeriod { get; set; }

        public decimal? Eccentricity { get; set; }

        public decimal? StellarMagnitude { get; set; }

        public string? HostStarTemperature { get; set; }

        public string? HostStarMass { get; set; }

        public string? Remark { get; set; }
    }
}