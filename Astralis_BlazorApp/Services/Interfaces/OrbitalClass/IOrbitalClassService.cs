using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IOrbitalClassService
{
    Task<OrbitalClassDto?> GetByIdAsync(int id);
    Task<List<OrbitalClassDto>> GetAllAsync();
}