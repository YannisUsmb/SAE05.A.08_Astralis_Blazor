using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CometService(HttpClient httpClient) : ICometService
{
    private const string Controller = "Comets";
    
    public async Task<CometDto?> GetByIdAsync(int id)
    {
        CometDto? comet = await httpClient.GetFromJsonAsync<CometDto>($"{Controller}/{id}");
        return comet ?? throw new Exception("Comet not found");
    }

    public async Task<List<CometDto>> GetAllAsync()
    {
        List<CometDto>? comets = await httpClient.GetFromJsonAsync<List<CometDto>>(Controller);
        return comets ?? new List<CometDto>();
    }

    public async Task<CometDto?> AddAsync(CometCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        CometDto? createdEntity = await response.Content.ReadFromJsonAsync<CometDto>();
        return createdEntity ?? throw new Exception("Error creating Comet");
    }

    public async Task<CometDto?> UpdateAsync(int id, CometUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        CometDto? updatedEntity = await response.Content.ReadFromJsonAsync<CometDto>();
        return updatedEntity ?? throw new Exception("Error updating Comet");
    }

    public async Task<CometDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<CometDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CometDto>> GetByReferenceAsync(string reference)
    {
        List<CometDto>? comets = await httpClient.GetFromJsonAsync<List<CometDto>>($"{Controller}/reference/{Uri.EscapeDataString(reference)}");
        return comets ?? new List<CometDto>();
    }

    public async Task<List<CometDto>> SearchAsync(CometFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<CometDto>? comets = await httpClient.GetFromJsonAsync<List<CometDto>>(url);
        return comets ?? new List<CometDto>();
    }

    private string ToQueryString(CometFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Reference))
            parameters.Add($"reference={Uri.EscapeDataString(filter.Reference)}");
        
        if (filter.MinEccentricity.HasValue) parameters.Add($"minEccentricity={filter.MinEccentricity}");
        if (filter.MaxEccentricity.HasValue) parameters.Add($"maxEccentricity={filter.MaxEccentricity}");
        
        if (filter.MinInclination.HasValue) parameters.Add($"minInclination={filter.MinInclination}");
        if (filter.MaxInclination.HasValue) parameters.Add($"maxInclination={filter.MaxInclination}");
        
        if (filter.MinPerihelionAU.HasValue) parameters.Add($"minPerihelionAU={filter.MinPerihelionAU}");
        if (filter.MaxPerihelionAU.HasValue) parameters.Add($"maxPerihelionAU={filter.MaxPerihelionAU}");
        
        if (filter.MinAphelionAU.HasValue) parameters.Add($"minAphelionAU={filter.MinAphelionAU}");
        if (filter.MaxAphelionAU.HasValue) parameters.Add($"maxAphelionAU={filter.MaxAphelionAU}");
        
        if (filter.MinOrbitalPeriod.HasValue) parameters.Add($"minOrbitalPeriod={filter.MinOrbitalPeriod}");
        if (filter.MaxOrbitalPeriod.HasValue) parameters.Add($"maxOrbitalPeriod={filter.MaxOrbitalPeriod}");

        if (filter.MinMOID.HasValue) parameters.Add($"minMOID={filter.MinMOID}");
        if (filter.MaxMOID.HasValue) parameters.Add($"maxMOID={filter.MaxMOID}");

        return string.Join("&", parameters);
    }
}