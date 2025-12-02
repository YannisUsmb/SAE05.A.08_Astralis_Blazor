using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.Planet;

public interface IPlanetService
{
    Task<PlanetDto?> GetByIdAsync(int id);
    Task<List<PlanetDto>> GetAllAsync();
    Task<PlanetDto?> AddAsync(PlanetCreateDto planet);
    Task<PlanetDto?> UpdateAsync(int id, PlanetUpdateDto planet);
    Task<PlanetDto?> DeleteAsync(int id);
    Task<List<PlanetDto>> SearchAsync(PlanetFilterDto filter);
}