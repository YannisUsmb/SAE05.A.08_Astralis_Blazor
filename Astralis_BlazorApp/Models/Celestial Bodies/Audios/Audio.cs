namespace Astralis_BlazorApp.Models
{
    public class Audio
    {
        public int Id { get; set; }
   
        public int CelestialBodyTypeId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string FilePath { get; set; } = null!;
    }
}