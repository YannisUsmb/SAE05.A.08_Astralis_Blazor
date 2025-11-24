using Astralis_BlazorApp.Models;
using Astralis_BlazorApp.Services.Interfaces;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class WebService<TEntity, TIdentifier> : IWriteableService<TEntity, TIdentifier>, IReadableService<TEntity, TIdentifier>
        where TEntity : class, IEntity<TIdentifier>
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _controllerName;

        public WebService(HttpClient httpClient, string controllerName)
        {
            _httpClient = httpClient;
            _controllerName = controllerName.TrimEnd('/');
        }

        public virtual async Task<IEnumerable<TEntity>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TEntity>>($"{_controllerName}/GetAll");
        }

        public virtual async Task<TEntity?> GetByIdAsync(TIdentifier id)
        {
            return await _httpClient.GetFromJsonAsync<TEntity>($"{_controllerName}/GetById/{id}");
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _httpClient.PostAsJsonAsync($"{_controllerName}/Post", entity);
        }

        public virtual async Task UpdateAsync(TEntity updatedEntity)
        {
            await _httpClient.PutAsJsonAsync($"{_controllerName}/Put/{updatedEntity.Id}", updatedEntity);
        }

        public virtual async Task DeleteAsync(TIdentifier id)
        {
            await _httpClient.DeleteAsync($"{_controllerName}/Delete/{id}");
        }
    }
}