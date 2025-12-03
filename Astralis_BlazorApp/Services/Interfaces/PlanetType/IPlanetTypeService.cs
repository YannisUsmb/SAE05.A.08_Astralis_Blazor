using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IPlanetTypeService
{
    Task<PlanetTypeDto?> GetByIdAsync(int id);
    Task<List<PlanetTypeDto>> GetAllAsync();
}