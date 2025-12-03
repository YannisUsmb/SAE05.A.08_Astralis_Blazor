using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface INotificationService
{
    Task<NotificationDto?> GetByIdAsync(int id);
    Task<List<NotificationDto>> GetAllAsync();
    Task<NotificationDto?> AddAsync(NotificationCreateDto dto);
    Task<NotificationDto?> UpdateAsync(int id, NotificationUpdateDto dto);
    Task<NotificationDto?> DeleteAsync(int id);
}