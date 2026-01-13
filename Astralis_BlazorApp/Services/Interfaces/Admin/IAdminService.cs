using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IAdminService
{
    Task<List<DiscoveryDto>> GetPendingDiscoveriesAsync();
    
    Task ApproveDiscoveryAsync(int id);
    
    Task RejectDiscoveryAsync(int id, string reason);
}