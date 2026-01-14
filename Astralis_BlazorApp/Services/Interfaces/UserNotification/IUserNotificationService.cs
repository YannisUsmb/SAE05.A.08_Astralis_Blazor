using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserNotificationService
{
    Task<List<UserNotificationDto>> GetAllAsync(int userId);
    Task<UserNotificationDto?> GetByIdAsync(int id);
    Task<UserNotificationDto?> AddAsync(UserNotificationCreateDto dto);
    Task<UserNotificationDto?> UpdateAsync(int id, UserNotificationUpdateDto dto);
    Task<UserNotificationDto?> DeleteAsync(int id);
}