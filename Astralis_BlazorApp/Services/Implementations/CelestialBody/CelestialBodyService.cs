using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.CelestialBody;

public class CelestialBodyService(HttpClient httpClient) : ICelestialBodyService
{
    private const string Controller = "CelestialBodies";
    
    public async Task<List<CelestialBodyListDto>> GetAllAsync()
    {
        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>(Controller);
        return entities ?? new List<CelestialBodyListDto>();
    }

    public async Task<CelestialBodyListDto?> AddAsync(CelestialBodyCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        CelestialBodyListDto? createdEntity = await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        return createdEntity ?? throw new Exception("Error creating Celestial Body");
    }

    public async Task<CelestialBodyListDto?> UpdateAsync(int id, CelestialBodyUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        CelestialBodyListDto? updatedEntity = await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        return updatedEntity ?? throw new Exception("Error updating Celestial Body");
    }

    public async Task<CelestialBodyListDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CelestialBodyListDto>> GetByNameAsync(string name)
    {
        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>($"{Controller}/reference/{Uri.EscapeDataString(name)}");
        return entities ?? new List<CelestialBodyListDto>();
    }

    public async Task<List<CelestialBodyListDto>> SearchAsync(CelestialBodyFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>(url);
        return entities ?? new List<CelestialBodyListDto>();
    }

    private string ToQueryString(CelestialBodyFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            parameters.Add($"searchText={Uri.EscapeDataString(filter.SearchText)}");
        
        if (filter.CelestialBodyTypeIds is { Count: > 0 })
        {
            parameters.AddRange(filter.CelestialBodyTypeIds.Select(id => $"celestialBodyTypeIds={id}"));
        }

        if (filter.IsDiscovery.HasValue)
            parameters.Add($"isDiscovery={filter.IsDiscovery.Value}");

        return string.Join("&", parameters);
    }
}