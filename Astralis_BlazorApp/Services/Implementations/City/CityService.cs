using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CityService(HttpClient httpClient) : ICityService
{
    private const string Controller = "Cities";

    public async Task<CityDto?> GetByIdAsync(int id)
    {
        CityDto? city = await httpClient.GetFromJsonAsync<CityDto>($"{Controller}/{id}");
        return city ?? throw new Exception("City not found");
    }

    public async Task<List<CityDto>> GetAllAsync()
    {
        List<CityDto>? cities = await httpClient.GetFromJsonAsync<List<CityDto>>(Controller);
        return cities ?? new List<CityDto>();
    }
    public async Task<List<CityDto>> SearchAsync(string term)
    {
        var result = await httpClient.GetFromJsonAsync<List<CityDto>>($"Cities/search/{term}");
        return result ?? new List<CityDto>();
    }
    public async Task<CityDto?> AddAsync(CityCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        CityDto? createdEntity = await response.Content.ReadFromJsonAsync<CityDto>();
        return createdEntity ?? throw new Exception("Error creating City");
    }
}