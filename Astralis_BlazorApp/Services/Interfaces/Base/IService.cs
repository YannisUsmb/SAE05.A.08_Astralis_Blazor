using Astralis_BlazorApp.Services.Interfaces;

namespace BlazorApp.Services.Interfaces
{
    public interface IService<TEntity, in TIdentifier, in TKey>
        : ISearchableService<TEntity, TKey>,
            IReadableService<TEntity, TIdentifier>,
            IWriteableService<TEntity, TIdentifier>;
}