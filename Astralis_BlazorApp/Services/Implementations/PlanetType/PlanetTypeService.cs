using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class PlanetTypeService(HttpClient httpClient) : IPlanetTypeService
{
    private const string Controller = "PlanetTypes";
    
    public async Task<PlanetTypeDto?> GetByIdAsync(int id)
    {
        PlanetTypeDto? planetType = await httpClient.GetFromJsonAsync<PlanetTypeDto>($"{Controller}/{id}");
        return planetType;
    }
    
    public async Task<List<PlanetTypeDto>> GetAllAsync()
    {
        List<PlanetTypeDto>? planetTypes = await httpClient.GetFromJsonAsync<List<PlanetTypeDto>>(Controller);
        return planetTypes ?? new List<PlanetTypeDto>();
    }
}