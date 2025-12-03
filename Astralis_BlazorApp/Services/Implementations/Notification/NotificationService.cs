using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class NotificationService(HttpClient httpClient) : INotificationService
{
    private const string Controller = "Notifications";
    
    public async Task<NotificationDto?> GetByIdAsync(int id)
    {
        NotificationDto? notification = await httpClient.GetFromJsonAsync<NotificationDto>($"{Controller}/{id}");
        return notification;
    }
    
    public async Task<List<NotificationDto>> GetAllAsync()
    {
        List<NotificationDto>? notifications = await httpClient.GetFromJsonAsync<List<NotificationDto>>(Controller);
        return notifications ?? new List<NotificationDto>();
    }

    public async Task<NotificationDto?> AddAsync(NotificationCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NotificationDto>();
        }
        return null;
    }

    public async Task<NotificationDto?> UpdateAsync(int id, NotificationUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NotificationDto>();
        }
        return null;
    }

    public async Task<NotificationDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NotificationDto>();
        }
        return null;
    }
}