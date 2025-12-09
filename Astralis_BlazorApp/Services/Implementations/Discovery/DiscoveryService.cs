using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class DiscoveryService(HttpClient httpClient) : IDiscoveryService
{
    private const string Controller = "Discoveries"; 

    public async Task<DiscoveryDto?> GetByIdAsync(int id)
    {
        DiscoveryDto? discovery = await httpClient.GetFromJsonAsync<DiscoveryDto>($"{Controller}/{id}");
        return discovery ?? throw new Exception("Discovery not found");
    }

    public async Task<List<DiscoveryDto>> GetAllAsync()
    {
        List<DiscoveryDto>? discoveries = await httpClient.GetFromJsonAsync<List<DiscoveryDto>>(Controller);
        return discoveries ?? new List<DiscoveryDto>();
    }

    public async Task<DiscoveryDto?> CreateAsteroidAsync(DiscoveryAsteroidSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Asteroids", submission);
        response.EnsureSuccessStatusCode();

        DiscoveryDto? createdEntity = await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        return createdEntity ?? throw new Exception("Error creating Asteroid discovery");
    }

    public async Task<DiscoveryDto?> CreatePlanetAsync(DiscoveryPlanetSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Planets", submission);
        response.EnsureSuccessStatusCode();

        DiscoveryDto? createdEntity = await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        return createdEntity ?? throw new Exception("Error creating Planet discovery");
    }

    public async Task<DiscoveryDto?> CreateStarAsync(DiscoveryStarSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Stars", submission);
        response.EnsureSuccessStatusCode();

        DiscoveryDto? createdEntity = await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        return createdEntity ?? throw new Exception("Error creating Star discovery");
    }

    public async Task<DiscoveryDto?> CreateCometAsync(DiscoveryCometSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Comets", submission);
        response.EnsureSuccessStatusCode();

        DiscoveryDto? createdEntity = await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        return createdEntity ?? throw new Exception("Error creating Comet discovery");
    }

    public async Task<DiscoveryDto?> CreateGalaxyAsync(DiscoveryGalaxyQuasarSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/GalaxyQuasars", submission);
        response.EnsureSuccessStatusCode();

        DiscoveryDto? createdEntity = await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        return createdEntity ?? throw new Exception("Error creating Galaxy discovery");
    }

    public async Task UpdateTitleAsync(int id, DiscoveryUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task ProposeAliasAsync(int id, DiscoveryAliasDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/Alias", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task ModerateStatusAsync(int id, DiscoveryModerationDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/Status", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<DiscoveryDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
        
        try 
        {
            return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        }
        catch 
        {
            return null; 
        }
    }

    public async Task<List<DiscoveryDto>> SearchAsync(DiscoveryFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<DiscoveryDto>? discoveries = await httpClient.GetFromJsonAsync<List<DiscoveryDto>>(url);
        return discoveries ?? new List<DiscoveryDto>();
    }

    // --- HELPER (Construction manuelle QueryString) ---

    private string ToQueryString(DiscoveryFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Title))
            parameters.Add($"title={Uri.EscapeDataString(filter.Title)}");

        if (filter.DiscoveryStatusId.HasValue)
            parameters.Add($"discoveryStatusId={filter.DiscoveryStatusId}");

        if (filter.AliasStatusId.HasValue)
            parameters.Add($"aliasStatusId={filter.AliasStatusId}");

        if (filter.DiscoveryApprovalUserId.HasValue)
            parameters.Add($"discoveryApprovalUserId={filter.DiscoveryApprovalUserId}");

        if (filter.AliasApprovalUserId.HasValue)
            parameters.Add($"aliasApprovalUserId={filter.AliasApprovalUserId}");

        return string.Join("&", parameters);
    }
}