namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IWriteableService<TEntity, in TIdentifier>
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity updatedEntity);
        Task DeleteAsync(TIdentifier id);
    }
}