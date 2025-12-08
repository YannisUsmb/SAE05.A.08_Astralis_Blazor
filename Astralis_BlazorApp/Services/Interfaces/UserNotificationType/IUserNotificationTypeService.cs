using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserNotificationTypeService
{
    Task<UserNotificationTypeDto?> GetByIdAsync(int userId, int notificationTypeId);
    Task<List<UserNotificationTypeDto>> GetAllAsync();
    Task<UserNotificationTypeDto?> AddAsync(UserNotificationTypeCreateDto dto);
    Task<UserNotificationTypeDto?> UpdateAsync(int userId, int notificationTypeId, UserNotificationTypeUpdateDto dto);
    Task<UserNotificationTypeDto?> DeleteAsync(int userId, int notificationTypeId);
}