using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IAsteroidService
{
    Task<AsteroidDto?> GetByIdAsync(int id);
    Task<List<AsteroidDto>> GetAllAsync();
    Task<List<AsteroidDto>> SearchAsync(AsteroidFilterDto filter);
    Task UpdateAsync(int id, AsteroidUpdateDto dto);
    Task DeleteAsync(int id);
}