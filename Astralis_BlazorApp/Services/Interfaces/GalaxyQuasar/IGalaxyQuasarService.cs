using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IGalaxyQuasarService
{
    Task<GalaxyQuasarDto?> GetByIdAsync(int id);
    Task<List<GalaxyQuasarDto>> GetAllAsync();
    Task<GalaxyQuasarDto?> AddAsync(GalaxyQuasarCreateDto dto);
    Task<GalaxyQuasarDto?> UpdateAsync(int id, GalaxyQuasarUpdateDto dto);
    Task<GalaxyQuasarDto?> DeleteAsync(int id);
    Task<List<GalaxyQuasarDto>> GetByReferenceAsync(string reference);
    Task<List<GalaxyQuasarDto>> SearchAsync(GalaxyQuasarFilterDto filter);
}