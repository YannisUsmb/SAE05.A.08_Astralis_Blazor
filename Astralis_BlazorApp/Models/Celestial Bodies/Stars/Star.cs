namespace Astralis_BlazorApp.Models
{
    public class Star
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int? SpectralClassId { get; set; }

        public string? Designation { get; set; }

        public DateOnly? ApprovalDate { get; set; }

        public string? Constellation { get; set; }

        public string? BayerDesignation { get; set; }

        public decimal? Distance { get; set; }

        public decimal? Luminosity { get; set; }

        public decimal? Radius { get; set; }

        public decimal? Temperature { get; set; }
    }
}