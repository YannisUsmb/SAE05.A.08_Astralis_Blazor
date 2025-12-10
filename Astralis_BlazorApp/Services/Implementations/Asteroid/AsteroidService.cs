using System.Globalization;
using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class AsteroidService(HttpClient httpClient) : IAsteroidService
{
    private const string Controller = "Asteroids";

    public async Task<AsteroidDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<AsteroidDto>($"{Controller}/{id}");
    }

    public async Task<List<AsteroidDto>> GetAllAsync()
    {
        List<AsteroidDto>? result = await httpClient.GetFromJsonAsync<List<AsteroidDto>>(Controller);
        return result ?? new List<AsteroidDto>();
    }

    public async Task UpdateAsync(int id, AsteroidUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AsteroidDto>> SearchAsync(AsteroidFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<AsteroidDto>? result = await httpClient.GetFromJsonAsync<List<AsteroidDto>>(url);
        return result ?? new List<AsteroidDto>();
    }

    private string ToQueryString(AsteroidFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Reference))
            parameters.Add($"reference={Uri.EscapeDataString(filter.Reference)}");

        if (filter.IsPotentiallyHazardous.HasValue)
            parameters.Add($"isPotentiallyHazardous={filter.IsPotentiallyHazardous.Value}");

        if (filter.OrbitId.HasValue)
            parameters.Add($"orbitId={filter.OrbitId.Value}");

        if (filter.OrbitalClassIds is { Count: > 0 })
        {
            parameters.AddRange(filter.OrbitalClassIds.Select(id => $"orbitalClassIds={id}"));
        }
        
        if (filter.MinAbsoluteMagnitude.HasValue)
            parameters.Add($"minAbsoluteMagnitude={filter.MinAbsoluteMagnitude.Value.ToString(CultureInfo.InvariantCulture)}");
        if (filter.MaxAbsoluteMagnitude.HasValue)
            parameters.Add($"maxAbsoluteMagnitude={filter.MaxAbsoluteMagnitude.Value.ToString(CultureInfo.InvariantCulture)}");

        if (filter.MinDiameter.HasValue)
            parameters.Add($"minDiameter={filter.MinDiameter.Value.ToString(CultureInfo.InvariantCulture)}");
        if (filter.MaxDiameter.HasValue)
            parameters.Add($"maxDiameter={filter.MaxDiameter.Value.ToString(CultureInfo.InvariantCulture)}");

        if (filter.MinInclination.HasValue)
            parameters.Add($"minInclination={filter.MinInclination.Value.ToString(CultureInfo.InvariantCulture)}");
        if (filter.MaxInclination.HasValue)
            parameters.Add($"maxInclination={filter.MaxInclination.Value.ToString(CultureInfo.InvariantCulture)}");

        if (filter.MinSemiMajorAxis.HasValue)
            parameters.Add($"minSemiMajorAxis={filter.MinSemiMajorAxis.Value.ToString(CultureInfo.InvariantCulture)}");
        if (filter.MaxSemiMajorAxis.HasValue)
            parameters.Add($"maxSemiMajorAxis={filter.MaxSemiMajorAxis.Value.ToString(CultureInfo.InvariantCulture)}");

        return string.Join("&", parameters);
    }
}