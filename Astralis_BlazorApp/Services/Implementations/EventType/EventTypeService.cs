using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class EventTypeService(HttpClient httpClient) : IEventTypeService
{
    private const string Controller = "EventTypes";
    
    public async Task<EventTypeDto?> GetByIdAsync(int id)
    {
        EventTypeDto? galaxyQuasarClass = await httpClient.GetFromJsonAsync<EventTypeDto>($"{Controller}/{id}");
        return galaxyQuasarClass;
    }
    
    public async Task<List<EventTypeDto>> GetAllAsync()
    {
        List<EventTypeDto>? galaxyQuasarClasses = await httpClient.GetFromJsonAsync<List<EventTypeDto>>($"{Controller}");
        return galaxyQuasarClasses ?? new List<EventTypeDto>();
    }

    public async Task<EventTypeDto> AddAsync(EventTypeCreateDto dto)
    {
        var response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventTypeDto>()
               ?? throw new Exception("Erreur lors de la création du type.");
    }
}