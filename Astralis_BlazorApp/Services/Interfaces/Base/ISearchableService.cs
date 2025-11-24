namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface ISearchableService<TEntity, in TKey>
    {
        Task<IEnumerable<TEntity?>> GetByKeyAsync(TKey key);
    }
}