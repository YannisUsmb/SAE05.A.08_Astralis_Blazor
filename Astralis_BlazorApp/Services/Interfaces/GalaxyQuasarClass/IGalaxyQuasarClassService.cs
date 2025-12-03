using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.GalaxyQuasarClass;

public interface IGalaxyQuasarClassService
{
    Task<GalaxyQuasarClassDto?> GetByIdAsync(int id);
    Task<List<GalaxyQuasarClassDto>> GetAllAsync();
}