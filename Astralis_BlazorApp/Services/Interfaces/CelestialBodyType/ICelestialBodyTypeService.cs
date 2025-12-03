using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICelestialBodyTypeService
{
    Task<CelestialBodyTypeDto?> GetByIdAsync(int id);
    Task<List<CelestialBodyTypeDto>> GetAllAsync();
}