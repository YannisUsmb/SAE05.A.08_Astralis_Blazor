using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface INotificationTypeService
{
    Task<NotificationTypeDto?> GetByIdAsync(int id);
    Task<List<NotificationTypeDto>> GetAllAsync();
}