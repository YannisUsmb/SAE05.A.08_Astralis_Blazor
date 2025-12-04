using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserNotificationService
{
    Task<UserNotificationDto?> GetByIdAsync(int userId, int notificationId);
    Task<List<UserNotificationDto>> GetAllAsync(int userId);
    Task<UserNotificationDto?> AddAsync(UserNotificationCreateDto dto);
    Task<UserNotificationDto?> UpdateAsync(int userId, int notificationId, UserNotificationUpdateDto dto);
    Task<UserNotificationDto?> DeleteAsync(int userId, int notificationId);
}