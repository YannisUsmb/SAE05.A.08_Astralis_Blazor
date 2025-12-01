using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.Star;

public class StarService(HttpClient httpClient) : IStarService
{
    private const string Controller = "Stars";

    public async Task<StarDto> GetStarById(int id)
    {
        StarDto? star = await httpClient.GetFromJsonAsync<StarDto>($"{Controller}/{id}");
        return star ?? throw new Exception($"{Controller} not found");
    }

    public async Task<List<StarDto>> GetAllStars()
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>(Controller);
        return stars ?? new List<StarDto>();
    }
    
    public async Task<StarDto> AddStar(StarCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to add star");
    }

    public async Task<StarDto> UpdateStar(int id, StarUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to update star");
    }

    public async Task<StarDto> DeleteStar(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StarDto>()
               ?? throw new Exception("Unable to delete star");
    }

    public async Task<List<StarDto>> GetByReference(string reference)
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>
            ($"{Controller}/reference/{reference}");

        return stars ?? new List<StarDto>();
    }

    public async Task<List<StarDto>> GetBySpectralClassId(int id)
    {
        List<StarDto>? stars = await httpClient.GetFromJsonAsync<List<StarDto>>
            ($"{Controller}/spectral-class/{id}");

        return stars ?? new List<StarDto>();
    }
}