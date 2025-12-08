using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class EventService(HttpClient httpClient) : IEventService 
{
    private const string Controller = "Events";
    
    public async Task<EventDto?> GetByIdAsync(int id)
    {
        EventDto? eventDto = await httpClient.GetFromJsonAsync<EventDto>($"{Controller}/{id}");
        return eventDto ?? throw new Exception("Event not found");
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        List<EventDto>? events = await httpClient.GetFromJsonAsync<List<EventDto>>(Controller);
        return events ?? new List<EventDto>();
    }

    public async Task<EventDto?> AddAsync(EventCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        EventDto? createdEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        return createdEvent ?? throw new Exception("Error creating event");
    }

    public async Task<EventDto?> UpdateAsync(int id, EventUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        EventDto? updatedEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        return updatedEvent ?? throw new Exception("Error updating event");
    }

    public async Task<EventDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        EventDto? deletedEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        return deletedEvent ?? throw new Exception("Error deleting event");
    }

    public async Task<List<EventDto>> GetByTitleAsync(string title)
    {
        List<EventDto>? events = await httpClient.GetFromJsonAsync<List<EventDto>>($"{Controller}/search/title/{title}");
        return events ?? new List<EventDto>();
    }

    public async Task<List<EventDto>> SearchAsync(EventFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/search?{queryString}";

        List<EventDto>? events = await httpClient.GetFromJsonAsync<List<EventDto>>(url);
        return events ?? new List<EventDto>();
    }

    private string ToQueryString(EventFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            parameters.Add($"searchText={Uri.EscapeDataString(filter.SearchText)}");
        
        if (filter.EventTypeIds is { Count: > 0 })
            parameters.AddRange(filter.EventTypeIds.Select(id => $"eventTypeIds={id}"));
        
        if (filter.MinStartDate.HasValue)
            parameters.Add($"minStartDate={Uri.EscapeDataString(filter.MinStartDate.Value.ToString("O"))}");

        if (filter.MaxStartDate.HasValue)
            parameters.Add($"maxStartDate={Uri.EscapeDataString(filter.MaxStartDate.Value.ToString("O"))}");

        if (filter.MinEndDate.HasValue)
            parameters.Add($"minEndDate={Uri.EscapeDataString(filter.MinEndDate.Value.ToString("O"))}");

        if (filter.MaxEndDate.HasValue)
            parameters.Add($"maxEndDate={Uri.EscapeDataString(filter.MaxEndDate.Value.ToString("O"))}");

        return string.Join("&", parameters);
    }
}