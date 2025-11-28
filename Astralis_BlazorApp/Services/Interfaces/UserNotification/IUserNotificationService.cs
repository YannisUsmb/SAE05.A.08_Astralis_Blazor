using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserNotificationService
{
    Task<UserNotificationDto> GetUserNotification(int userId, int notificationId);
    Task<List<UserNotificationDto>> GetUserNotifications(int userId);
    Task<UserNotificationDto> AddUserNotification(UserNotificationDto dto);
    Task<UserNotificationDto> UpdateUserNotification(UserNotificationDto dto);
    Task<UserNotificationDto> DeleteUserNotification(int userId, int notificationId);
}