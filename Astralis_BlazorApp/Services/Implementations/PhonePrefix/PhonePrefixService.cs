using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class PhonePrefixService(HttpClient httpClient) : IPhonePrefixService
{
    private const string Controller = "PhonePrefixes";
    
    public async Task<PhonePrefixDto?> GetByIdAsync(int id)
    {
        PhonePrefixDto? phonePrefix = await httpClient.GetFromJsonAsync<PhonePrefixDto>($"{Controller}/{id}");
        return phonePrefix;
    }
    
    public async Task<List<PhonePrefixDto>> GetAllAsync()
    {
        List<PhonePrefixDto>? phonePrefixes = await httpClient.GetFromJsonAsync<List<PhonePrefixDto>>(Controller);
        return phonePrefixes ?? new List<PhonePrefixDto>();
    }
}