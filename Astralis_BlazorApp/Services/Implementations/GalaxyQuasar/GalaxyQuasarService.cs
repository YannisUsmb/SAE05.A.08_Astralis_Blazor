using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class GalaxyQuasarService(HttpClient httpClient) : IGalaxyQuasarService
{
    private const string Controller = "GalaxyQuasars";
    
    public async Task<GalaxyQuasarDto?> GetByIdAsync(int id)
    {
        GalaxyQuasarDto? galaxyQuasar = await httpClient.GetFromJsonAsync<GalaxyQuasarDto>($"{Controller}/{id}");
        return galaxyQuasar ?? throw new Exception("Galaxy/Quasar not found");
    }

    public async Task<List<GalaxyQuasarDto>> GetAllAsync()
    {
        List<GalaxyQuasarDto>? galaxyQuasars = await httpClient.GetFromJsonAsync<List<GalaxyQuasarDto>>(Controller);
        return galaxyQuasars ?? new List<GalaxyQuasarDto>();
    }

    public async Task<GalaxyQuasarDto?> AddAsync(GalaxyQuasarCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        GalaxyQuasarDto? createdEntity = await response.Content.ReadFromJsonAsync<GalaxyQuasarDto>();
        return createdEntity ?? throw new Exception("Error creating Galaxy/Quasar");
    }

    public async Task<GalaxyQuasarDto?> UpdateAsync(int id, GalaxyQuasarUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        GalaxyQuasarDto? updatedEntity = await response.Content.ReadFromJsonAsync<GalaxyQuasarDto>();
        return updatedEntity ?? throw new Exception("Error updating Galaxy/Quasar");
    }

    public async Task<GalaxyQuasarDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        GalaxyQuasarDto? deletedEntity = await response.Content.ReadFromJsonAsync<GalaxyQuasarDto>();
        return deletedEntity ?? throw new Exception("Error deleting Galaxy/Quasar");
    }

    public async Task<List<GalaxyQuasarDto>> GetByReferenceAsync(string reference)
    {
        List<GalaxyQuasarDto>? galaxyQuasars = await httpClient.GetFromJsonAsync<List<GalaxyQuasarDto>>($"{Controller}/reference/{reference}");
        return galaxyQuasars ?? new List<GalaxyQuasarDto>();
    }

    public async Task<List<GalaxyQuasarDto>> SearchAsync(GalaxyQuasarFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/search?{queryString}";

        List<GalaxyQuasarDto>? galaxyQuasars = await httpClient.GetFromJsonAsync<List<GalaxyQuasarDto>>(url);
        return galaxyQuasars ?? new List<GalaxyQuasarDto>();
    }

    private string ToQueryString(GalaxyQuasarFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Reference))
            parameters.Add($"reference={Uri.EscapeDataString(filter.Reference)}");

        // List filter using LINQ
        if (filter.GalaxyQuasarClassIds is { Count: > 0 })
            parameters.AddRange(filter.GalaxyQuasarClassIds.Select(id => $"galaxyQuasarClassIds={id}"));

        // Coordinates & Scientific Data
        if (filter.MinRightAscension.HasValue)
            parameters.Add($"minRightAscension={filter.MinRightAscension}");
        if (filter.MaxRightAscension.HasValue)
            parameters.Add($"maxRightAscension={filter.MaxRightAscension}");

        if (filter.MinDeclination.HasValue)
            parameters.Add($"minDeclination={filter.MinDeclination}");
        if (filter.MaxDeclination.HasValue)
            parameters.Add($"maxDeclination={filter.MaxDeclination}");

        if (filter.MinRedshift.HasValue)
            parameters.Add($"minRedshift={filter.MinRedshift}");
        if (filter.MaxRedshift.HasValue)
            parameters.Add($"maxRedshift={filter.MaxRedshift}");

        if (filter.MinRMagnitude.HasValue)
            parameters.Add($"minRMagnitude={filter.MinRMagnitude}");
        if (filter.MaxRMagnitude.HasValue)
            parameters.Add($"maxRMagnitude={filter.MaxRMagnitude}");

        if (filter.MinMjdObs.HasValue)
            parameters.Add($"minMjdObs={filter.MinMjdObs}");
        if (filter.MaxMjdObs.HasValue)
            parameters.Add($"maxMjdObs={filter.MaxMjdObs}");

        return string.Join("&", parameters);
    }
}