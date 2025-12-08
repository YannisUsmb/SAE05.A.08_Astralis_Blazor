using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class EventInterestService(HttpClient httpClient) : IEventInterestService
{
    private const string Controller = "EventInterests";
    
    public async Task<EventInterestDto?> GetByIdAsync(int eventId, int userId)
    {
        EventInterestDto? eventInterest = await httpClient.GetFromJsonAsync<EventInterestDto>($"{Controller}/{eventId}/{userId}");
        return eventInterest;
    }

    public async Task<List<EventInterestDto>> GetAllAsync()
    {
        List<EventInterestDto>? eventInterests = await httpClient.GetFromJsonAsync<List<EventInterestDto>>(Controller);
        return eventInterests ?? new List<EventInterestDto>();
    }

    public async Task<EventInterestDto?> AddAsync(EventInterestDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);

        if (response.IsSuccessStatusCode)
        {
            EventInterestDto? createdEventInterest = await response.Content.ReadFromJsonAsync<EventInterestDto>();
            return createdEventInterest;
        }
        return null;
    }

    public async Task<EventInterestDto?> DeleteAsync(int eventId, int userId)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{eventId}/{userId}");

        if (response.IsSuccessStatusCode)
        {
            EventInterestDto? deletedEventInterest = await response.Content.ReadFromJsonAsync<EventInterestDto>();
            return deletedEventInterest;
        }
        return null;
    }
}