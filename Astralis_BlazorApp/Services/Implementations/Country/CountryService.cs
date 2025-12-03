using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CountryService(HttpClient httpClient) : ICountryService
{
    private const string Controller = "Countries";
    
    public async Task<CountryDto?> GetByIdAsync(int id)
    {
        CountryDto? country = await httpClient.GetFromJsonAsync<CountryDto>($"{Controller}/{id}");
        return country ?? throw new Exception("Country not found");
    }
    
    public async Task<List<CountryDto>> GetAllAsync()
    {
        List<CountryDto>? countries = await httpClient.GetFromJsonAsync<List<CountryDto>>($"{Controller}");
        return countries ?? new List<CountryDto>();
    }
}