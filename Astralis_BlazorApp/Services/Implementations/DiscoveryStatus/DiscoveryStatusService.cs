using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class DiscoveryStatusService(HttpClient httpClient) : IDiscoveryStatusService
{
    private const string Controller = "DiscoveryStatuses";
    
    public async Task<DiscoveryStatusDto?> GetByIdAsync(int id)
    {
        DiscoveryStatusDto? status = await httpClient.GetFromJsonAsync<DiscoveryStatusDto>($"{Controller}/{id}");
        return status;
    }
    
    public async Task<List<DiscoveryStatusDto>> GetAllAsync()
    {
        List<DiscoveryStatusDto>? statuses = await httpClient.GetFromJsonAsync<List<DiscoveryStatusDto>>($"{Controller}");
        return statuses ?? new List<DiscoveryStatusDto>();
    }
}