namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IReadableService<TEntity, in TIdentifier>
    {
        Task<IEnumerable<TEntity>?> GetAllAsync();
        Task<TEntity?> GetByIdAsync(TIdentifier id);
    }
}