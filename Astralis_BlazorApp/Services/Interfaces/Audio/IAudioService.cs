using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IAudioService
{
    Task<AudioDto?> GetByIdAsync(int id);
    Task<List<AudioDto>> GetAllAsync();
    Task<List<AudioDto>> GetByTitleAsync(string title);
    Task<List<AudioDto>> GetByCategoryIdAsync(int id);
    Task<List<AudioDto>> SearchAsync(AudioFilterDto filter);
}