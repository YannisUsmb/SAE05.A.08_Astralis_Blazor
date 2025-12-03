using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IDiscoveryStatusService
{
    Task<DiscoveryStatusDto?> GetByIdAsync(int id);
    Task<List<DiscoveryStatusDto>> GetAllAsync();
}