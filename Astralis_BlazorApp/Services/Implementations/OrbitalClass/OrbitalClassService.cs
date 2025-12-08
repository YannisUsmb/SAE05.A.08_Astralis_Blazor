using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class OrbitalClassService(HttpClient httpClient) : IOrbitalClassService
{
    private const string Controller = "OrbitalClasses";
    
    public async Task<OrbitalClassDto?> GetByIdAsync(int id)
    {
        OrbitalClassDto? orbitalClass = await httpClient.GetFromJsonAsync<OrbitalClassDto>($"{Controller}/{id}");
        return orbitalClass;
    }
    
    public async Task<List<OrbitalClassDto>> GetAllAsync()
    {
        List<OrbitalClassDto>? orbitalClasses = await httpClient.GetFromJsonAsync<List<OrbitalClassDto>>(Controller);
        return orbitalClasses ?? new List<OrbitalClassDto>();
    }
}