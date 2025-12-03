using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CelestialBodyTypeService(HttpClient httpClient) : ICelestialBodyTypeService
{
    private const string Controller = "CelestialBodyTypes";
    
    public async Task<CelestialBodyTypeDto?> GetByIdAsync(int id)
    {
        CelestialBodyTypeDto? celestialBodyType = await httpClient.GetFromJsonAsync<CelestialBodyTypeDto>($"{Controller}/{id}");
        return celestialBodyType ?? throw new Exception("Celestial body type not found");
    }
    
    public async Task<List<CelestialBodyTypeDto>> GetAllAsync()
    {
        List<CelestialBodyTypeDto>? celestialBodyTypes = await httpClient.GetFromJsonAsync<List<CelestialBodyTypeDto>>(Controller);
        return celestialBodyTypes ?? new List<CelestialBodyTypeDto>();
    }
    
}