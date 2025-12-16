using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

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
    
    public async Task<List<CelestialBodySubtypeDto>> GetSubtypesAsync(int mainTypeId)
    {
        // Appel au nouvel endpoint du controller
        List<CelestialBodySubtypeDto>? subtypes = await httpClient.GetFromJsonAsync<List<CelestialBodySubtypeDto>>($"{Controller}/Subtypes/{mainTypeId}");
        return subtypes ?? new List<CelestialBodySubtypeDto>();
    }

    // --- CORRECTION ICI ---
    public async Task<List<CelestialBodyListDto>> SearchAsync(CelestialBodyFilterDto filter, int pageNumber, int pageSize)
    {
        // On passe pageNumber et pageSize à la méthode privée
        string queryString = ToQueryString(filter, pageNumber, pageSize);
        string url = $"{Controller}/Search?{queryString}";

        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>(url);
        return entities ?? new List<CelestialBodyListDto>();
    }

    // --- MODIFICATION DE LA MÉTHODE PRIVÉE ---
    private string ToQueryString(CelestialBodyFilterDto filter, int pageNumber, int pageSize)
    {
        List<string> parameters = new List<string>();

        // 1. On ajoute la pagination dans la liste finale
        parameters.Add($"pageNumber={pageNumber}");
        parameters.Add($"pageSize={pageSize}");

        // 2. On ajoute les filtres
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            parameters.Add($"searchText={Uri.EscapeDataString(filter.SearchText)}");
        
        if (filter.CelestialBodyTypeIds is { Count: > 0 })
        {
            parameters.AddRange(filter.CelestialBodyTypeIds.Select(id => $"celestialBodyTypeIds={id}"));
        }

        if (filter.IsDiscovery.HasValue)
            parameters.Add($"isDiscovery={filter.IsDiscovery.Value}");

        if (filter.SubtypeId.HasValue && filter.SubtypeId.Value > 0)
        {
            parameters.Add($"subtypeId={filter.SubtypeId.Value}");
        }
        
        return string.Join("&", parameters);
    }
}