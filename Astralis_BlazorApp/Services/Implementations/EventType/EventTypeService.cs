using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.EventType;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.EventType;

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
}