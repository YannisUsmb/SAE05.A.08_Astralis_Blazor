using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IDetectionMethodService
{
    Task<DetectionMethodDto?> GetByIdAsync(int id);
    Task<List<DetectionMethodDto>> GetAllAsync();
}