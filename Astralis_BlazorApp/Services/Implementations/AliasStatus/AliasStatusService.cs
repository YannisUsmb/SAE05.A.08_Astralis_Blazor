using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.AliasStatus;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class AliasStatusService(HttpClient httpClient) : IAliasStatusService
{
    private const string Controller = "AliasStatuses";
    
    public async Task<AliasStatusDto?> GetByIdAsync(int id)
    {
        AliasStatusDto? aliasStatus = await httpClient.GetFromJsonAsync<AliasStatusDto>($"{Controller}/{id}");
        return aliasStatus;
    }
    
    public async Task<List<AliasStatusDto>> GetAllAsync()
    {
        List<AliasStatusDto>? aliasStatuses = await httpClient.GetFromJsonAsync<List<AliasStatusDto>>(Controller);
        return aliasStatuses ?? new List<AliasStatusDto>();
    }
}