using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CelestialBodyService(HttpClient httpClient) : ICelestialBodyService
{
    private const string Controller = "CelestialBodies";
    
    public async Task<List<CelestialBodyListDto>> GetAllAsync()
    {
        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>(Controller);
        return entities ?? new List<CelestialBodyListDto>();
    }

    public async Task<CelestialBodyListDto?> AddAsync(CelestialBodyCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        CelestialBodyListDto? createdEntity = await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        return createdEntity ?? throw new Exception("Error creating Celestial Body");
    }

    public async Task<CelestialBodyListDto?> UpdateAsync(int id, CelestialBodyUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        CelestialBodyListDto? updatedEntity = await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        return updatedEntity ?? throw new Exception("Error updating Celestial Body");
    }

    public async Task<CelestialBodyListDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<CelestialBodyListDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<CelestialBodyListDto>> GetByNameAsync(string name)
    {
        List<CelestialBodyListDto>? entities = await httpClient.GetFromJsonAsync<List<CelestialBodyListDto>>($"{Controller}/reference/{Uri.EscapeDataString(name)}");
        return entities ?? new List<CelestialBodyListDto>();
    }
    
    public async Task<List<CelestialBodySubtypeDto>> GetSubtypesAsync(int mainTypeId)
    {
        List<CelestialBodySubtypeDto>? subtypes = await httpClient.GetFromJsonAsync<List<CelestialBodySubtypeDto>>($"{Controller}/Subtypes/{mainTypeId}");
        return subtypes ?? new List<CelestialBodySubtypeDto>();
    }
    
    public async Task<List<CelestialBodyListDto>> SearchAsync(CelestialBodyFilterDto filter, int pageNumber, int pageSize)
    {
        string url = $"{Controller}/Search?pageNumber={pageNumber}&pageSize={pageSize}";
        
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, filter);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<CelestialBodyListDto>>() 
                   ?? new List<CelestialBodyListDto>();
        }
        
        return new List<CelestialBodyListDto>();
    }
    
    public async Task<CelestialBodyDetailDto?> GetDetailsByIdAsync(int id)
    {
        try
        {
            CelestialBodyDetailDto? details = await httpClient.GetFromJsonAsync<CelestialBodyDetailDto>($"{Controller}/{id}/Details");
            return details;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching celestial body details: {ex.Message}");
            return null;
        }
    }
    
}