using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class PlanetService(HttpClient httpClient) : IPlanetService
{
    private const string Controller = "Planets";

    public async Task<PlanetDto?> GetByIdAsync(int id)
    {
        PlanetDto? planet = await httpClient.GetFromJsonAsync<PlanetDto>($"{Controller}/{id}");
        return planet ?? throw new Exception("Planet not found");
    }

    public async Task<List<PlanetDto>> GetAllAsync()
    {
        List<PlanetDto>? planets = await httpClient.GetFromJsonAsync<List<PlanetDto>>(Controller);
        return planets ?? new List<PlanetDto>();
    }

    public async Task<PlanetDto?> AddAsync(PlanetCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        PlanetDto? createdPlanet = await response.Content.ReadFromJsonAsync<PlanetDto>();
        return createdPlanet ?? throw new Exception("Error creating planet");
    }

    public async Task<PlanetDto?> UpdateAsync(int id, PlanetUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        PlanetDto? updatedPlanet = await response.Content.ReadFromJsonAsync<PlanetDto>();
        return updatedPlanet ?? throw new Exception("Error updating planet");
    }

    public async Task<PlanetDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        PlanetDto? deletedPlanet = await response.Content.ReadFromJsonAsync<PlanetDto>();
        return deletedPlanet ?? throw new Exception("Error deleting planet");
    }

    public async Task<List<PlanetDto>> GetByNameAsync(string name)
    {
        List<PlanetDto>? planets = await httpClient.GetFromJsonAsync<List<PlanetDto>>($"{Controller}/search/name/{name}");
        return planets ?? new List<PlanetDto>();
    }

    public async Task<List<PlanetDto>> SearchAsync(PlanetFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        List<PlanetDto>? planets = await httpClient.GetFromJsonAsync<List<PlanetDto>>($"{Controller}/search?{queryString}");
        
        return planets ?? new List<PlanetDto>();
    }
    
    private string ToQueryString(PlanetFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.Name))
            parameters.Add($"name={Uri.EscapeDataString(filter.Name)}");
        
        if (filter.PlanetTypeIds != null)
        {
            foreach (int id in filter.PlanetTypeIds)
            {
                parameters.Add($"planetTypeIds={id}");
            }
        }

        if (filter.DetectionMethodIds != null)
        {
            foreach (int id in filter.DetectionMethodIds)
            {
                parameters.Add($"detectionMethodIds={id}");
            }
        }
        
        if (filter.MinDiscoveryYear.HasValue)
            parameters.Add($"minDiscoveryYear={filter.MinDiscoveryYear}");
        
        if (filter.MaxDiscoveryYear.HasValue)
            parameters.Add($"maxDiscoveryYear={filter.MaxDiscoveryYear}");

        if (filter.MinOrbitalPeriod.HasValue)
            parameters.Add($"minOrbitalPeriod={filter.MinOrbitalPeriod}");

        if (filter.MaxOrbitalPeriod.HasValue)
            parameters.Add($"maxOrbitalPeriod={filter.MaxOrbitalPeriod}");

        if (filter.MinEccentricity.HasValue)
            parameters.Add($"minEccentricity={filter.MinEccentricity}");

        if (filter.MaxEccentricity.HasValue)
            parameters.Add($"maxEccentricity={filter.MaxEccentricity}");
            
        if (filter.MinStellarMagnitude.HasValue)
            parameters.Add($"minStellarMagnitude={filter.MinStellarMagnitude}");
            
        if (filter.MaxStellarMagnitude.HasValue)
            parameters.Add($"maxStellarMagnitude={filter.MaxStellarMagnitude}");
        
        if (!string.IsNullOrEmpty(filter.Distance))
            parameters.Add($"distance={Uri.EscapeDataString(filter.Distance)}");
            
        if (!string.IsNullOrEmpty(filter.Temperature))
            parameters.Add($"temperature={Uri.EscapeDataString(filter.Temperature)}");
        
        return string.Join("&", parameters);
    }
}