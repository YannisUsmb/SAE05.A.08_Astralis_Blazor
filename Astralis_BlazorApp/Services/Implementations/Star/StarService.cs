using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class StarService(HttpClient httpClient) : IStarService
{
    private const string Controller = "Stars";

    public async Task<StarDto> GetByIdAsync(int id)
    {
        StarDto? star = await httpClient.GetFromJsonAsync<StarDto>($"{Controller}/{id}");
        return star ?? throw new Exception($"{Controller} not found");
    }

    public async Task<List<StarDto>> GetAllAsync()
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>(Controller);
        return stars ?? new List<StarDto>();
    }
    
    public async Task<StarDto> AddAsync(StarCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to add star");
    }

    public async Task<StarDto> UpdateAsync(int id, StarUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to update star");
    }

    public async Task<StarDto> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to delete star");
    }

    public async Task<List<StarDto>> GetByReferenceAsync(string reference)
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>
            ($"{Controller}/reference/{reference}");

        return stars ?? new List<StarDto>();
    }

    public async Task<List<StarDto>> GetBySpectralClassIdAsync(int id)
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>
            ($"{Controller}/spectral-class/{id}");

        return stars ?? new List<StarDto>();
    }
    
    public async Task<List<StarDto>> SearchAsync(StarFilterDto filter)
    {
        string queryString = BuildSearchQueryString(filter);
        string url = $"{Controller}/search?{queryString}";

        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>(url);
        return stars ?? new List<StarDto>();
    }
    
    private string BuildSearchQueryString(StarFilterDto filter)
    {
        List<string> queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            queryParams.Add($"name={Uri.EscapeDataString(filter.Name)}");

        if (!string.IsNullOrWhiteSpace(filter.Constellation))
            queryParams.Add($"constellation={Uri.EscapeDataString(filter.Constellation)}");

        if (!string.IsNullOrWhiteSpace(filter.Designation))
            queryParams.Add($"designation={Uri.EscapeDataString(filter.Designation)}");

        if (!string.IsNullOrWhiteSpace(filter.BayerDesignation))
            queryParams.Add($"bayerDesignation={Uri.EscapeDataString(filter.BayerDesignation)}");
        
        if (filter.MinDistance.HasValue)
            queryParams.Add($"minDistance={filter.MinDistance.Value}");
        if (filter.MaxDistance.HasValue)
            queryParams.Add($"maxDistance={filter.MaxDistance.Value}");

        if (filter.MinLuminosity.HasValue)
            queryParams.Add($"minLuminosity={filter.MinLuminosity.Value}");
        if (filter.MaxLuminosity.HasValue)
            queryParams.Add($"maxLuminosity={filter.MaxLuminosity.Value}");

        if (filter.MinRadius.HasValue)
            queryParams.Add($"minRadius={filter.MinRadius.Value}");
        if (filter.MaxRadius.HasValue)
            queryParams.Add($"maxRadius={filter.MaxRadius.Value}");

        if (filter.MinTemperature.HasValue)
            queryParams.Add($"minTemperature={filter.MinTemperature.Value}");
        if (filter.MaxTemperature.HasValue)
            queryParams.Add($"maxTemperature={filter.MaxTemperature.Value}");
        
        if (filter.SpectralClassIds != null && filter.SpectralClassIds.Count > 0)
        {
            foreach (int id in filter.SpectralClassIds)
            {
                queryParams.Add($"spectralClassIds={id}");
            }
        }

        return string.Join("&", queryParams);
    }
}