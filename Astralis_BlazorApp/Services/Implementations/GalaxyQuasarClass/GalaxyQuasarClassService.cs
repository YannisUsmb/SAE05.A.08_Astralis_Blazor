using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.GalaxyQuasarClass;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.GalaxyQuasarClass;

public class GalaxyQuasarClassService(HttpClient httpClient) : IGalaxyQuasarClassService
{
    private const string Controller = "GalaxyQuasarClasses";
    
    public async Task<GalaxyQuasarClassDto?> GetByIdAsync(int id)
    {
        GalaxyQuasarClassDto? galaxyQuasarClass = await httpClient.GetFromJsonAsync<GalaxyQuasarClassDto>($"{Controller}/{id}");
        return galaxyQuasarClass;
    }
    
    public async Task<List<GalaxyQuasarClassDto>> GetAllAsync()
    {
        List<GalaxyQuasarClassDto>? galaxyQuasarClasses = await httpClient.GetFromJsonAsync<List<GalaxyQuasarClassDto>>($"{Controller}");
        return galaxyQuasarClasses ?? new List<GalaxyQuasarClassDto>();
    }
}