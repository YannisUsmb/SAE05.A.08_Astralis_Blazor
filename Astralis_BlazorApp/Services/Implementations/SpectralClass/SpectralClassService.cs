using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.SpectralClass;

public class SpectralClassService(HttpClient httpClient) : ISpectralClassService
{
    private const string Controller = "SpectralClass";
    
    public async Task<SpectralClassDto> GetByIdAsync(int id)
    {
        SpectralClassDto? spectralClass = await httpClient.GetFromJsonAsync<SpectralClassDto>($"{Controller}/{id}");
        return spectralClass ?? throw new Exception("Role not found");
    }
    
    public async Task<List<SpectralClassDto>> GetAllAsync()
    {
        List<SpectralClassDto>? spectralClasses = await httpClient.GetFromJsonAsync<List<SpectralClassDto>>($"{Controller}");

        if (spectralClasses == null)
        {
            return new List<SpectralClassDto>();
        }

        return spectralClasses;
    }
}