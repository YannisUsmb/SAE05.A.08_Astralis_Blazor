namespace Astralis_BlazorApp.Models
{
    public class Discovery
    {
        public int Id { get; set; }

        public int CelestialBodyId { get; set; }

        public int DiscoveryStatusId { get; set; }

        public int? AliasStatusId { get; set; }

        public int UserId { get; set; }

        public int? DiscoveryApprovalUserId { get; set; }

        public int? AliasApprovalUserId { get; set; }

        public string Title { get; set; } = null!;
    }
}
