using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IGalaxyQuasarClassService
{
    Task<GalaxyQuasarClassDto?> GetByIdAsync(int id);
    Task<List<GalaxyQuasarClassDto>> GetAllAsync();
}