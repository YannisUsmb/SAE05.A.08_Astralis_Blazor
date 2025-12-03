using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ISpectralClassService
{
    Task<SpectralClassDto> GetByIdAsync(int id);
    Task<List<SpectralClassDto>> GetAllAsync();
}