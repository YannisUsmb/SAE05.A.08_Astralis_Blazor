namespace Astralis_BlazorApp.Models
{
    public class GalaxyQuasar
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int GalaxyQuasarClassId { get; set; }

        public string? Reference { get; set; }

        public decimal? RightAscension { get; set; }

        public decimal? Declination { get; set; }

        public decimal? Redshift { get; set; }

        public decimal? RMagnitude { get; set; }

        public int? ModifiedJulianDateObservation { get; set; }
    }
}