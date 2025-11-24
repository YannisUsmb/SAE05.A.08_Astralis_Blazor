namespace Astralis_BlazorApp.Models
{
    public interface IEntity<TIdentifier>
    {
        TIdentifier Id { get; set; }
    }
}
