using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class AudioService(HttpClient httpClient) : IAudioService
{
    private const string Controller = "Audios";
    
    public async Task<AudioDto?> GetByIdAsync(int id)
    {
        AudioDto? audio = await httpClient.GetFromJsonAsync<AudioDto>($"{Controller}/{id}");
        return audio;
    }
    
    public async Task<List<AudioDto>> GetAllAsync()
    {
        List<AudioDto>? audios = await httpClient.GetFromJsonAsync<List<AudioDto>>(Controller);
        return audios ?? new List<AudioDto>();
    }

    public async Task<List<AudioDto>> GetByTitleAsync(string title)
    {
        string encodedTitle = Uri.EscapeDataString(title);
        List<AudioDto>? audios = await httpClient.GetFromJsonAsync<List<AudioDto>>($"{Controller}/search/{encodedTitle}");
        return audios ?? new List<AudioDto>();
    }

    public async Task<List<AudioDto>> GetByCategoryIdAsync(int id)
    {
        List<AudioDto>? audios = await httpClient.GetFromJsonAsync<List<AudioDto>>($"{Controller}/category/{id}");
        return audios ?? new List<AudioDto>();
    }
    
    public async Task<List<AudioDto>> SearchAsync(AudioFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<AudioDto>? audios = await httpClient.GetFromJsonAsync<List<AudioDto>>(url);
        return audios ?? new List<AudioDto>();
    }
    
    private string ToQueryString(AudioFilterDto filter)
    {
        var parameters = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            parameters.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
        }
        
        if (filter.CelestialBodyTypeIds is { Count: > 0 })
        {
            parameters.AddRange(filter.CelestialBodyTypeIds.Select(id => $"celestialBodyTypeIds={id}"));
        }

        return string.Join("&", parameters);
    }
}