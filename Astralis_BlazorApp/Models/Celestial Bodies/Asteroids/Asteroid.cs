namespace Astralis_BlazorApp.Models
{
    public class Asteroid
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int OrbitalClassId { get; set; }

        public string? Reference { get; set; }

        public decimal? AbsoluteMagnitude { get; set; }

        public decimal? DiameterMinKm { get; set; }

        public decimal? DiameterMaxKm { get; set; }

        public bool? IsPotentiallyHazardous { get; set; }

        public int? OrbitId { get; set; }

        public DateTime? OrbitDeterminationDate { get; set; }

        public DateTime? FirstObservationDate { get; set; }

        public DateTime? LastObservationDate { get; set; }

        public decimal? SemiMajorAxis { get; set; }

        public decimal? Inclination { get; set; }
    }
}