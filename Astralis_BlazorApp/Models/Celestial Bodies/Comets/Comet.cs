namespace Astralis_BlazorApp.Models
{
    public class Comet
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public decimal? OrbitalEccentricity { get; set; }

        public decimal? OrbitalInclinationDegrees { get; set; }

        public decimal? AscendingNodeLongitudeDegrees { get; set; }

        public decimal? PerihelionDistanceAU { get; set; }

        public decimal? AphelionDistanceAU { get; set; }

        public decimal? OrbitalPeriodYears { get; set; }

        public decimal? MinimumOrbitIntersectionDistanceAU { get; set; }

        public string? Reference { get; set; }
    }
}
