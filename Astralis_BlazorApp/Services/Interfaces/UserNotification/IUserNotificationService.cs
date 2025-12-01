using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserNotificationService
{
    Task<UserNotificationDto> GetByIdAsync(int userId, int notificationId);
    Task<List<UserNotificationDto>> GetAllAsync(int userId);
    Task<UserNotificationDto> AddAsync(UserNotificationDto dto);
    Task<UserNotificationDto> UpdateAsync(int userId, int notificationId, UserNotificationDto dto);
    Task<UserNotificationDto> DeleteAsync(int userId, int notificationId);
}