using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class NotificationTypeService(HttpClient httpClient) : INotificationTypeService
{
    private const string Controller = "NotificationTypes";

    public async Task<NotificationTypeDto?> GetByIdAsync(int id)
    {
        NotificationTypeDto? notificationType = await httpClient.GetFromJsonAsync<NotificationTypeDto>($"{Controller}/{id}");
        return notificationType;
    }
    
    public async Task<List<NotificationTypeDto>> GetAllAsync()
    {
        List<NotificationTypeDto>? notificationTypes = await httpClient.GetFromJsonAsync<List<NotificationTypeDto>>($"{Controller}");
        return notificationTypes ?? new List<NotificationTypeDto>();
    }
}