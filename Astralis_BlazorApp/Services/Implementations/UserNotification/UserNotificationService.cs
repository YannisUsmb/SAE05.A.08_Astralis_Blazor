using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserNotificationService(HttpClient httpClient) : IUserNotificationService
    {
        private const string Controller = "users";
        
        public async Task<UserNotificationDto> GetUserNotification(int userId, int notificationId)
        {
            UserNotificationDto? notification = await httpClient.GetFromJsonAsync<UserNotificationDto>($"{Controller}/{userId}/notifications/{notificationId}");
            return notification!;
        }
        
        public async Task<List<UserNotificationDto>> GetUserNotifications(int userId)
        {
            List<UserNotificationDto>? notifications = await httpClient.GetFromJsonAsync<List<UserNotificationDto>>($"{Controller}/{userId}/notifications");

            if (notifications == null)
            {
                return new List<UserNotificationDto>();
            }

            return notifications;
        }
        
        public async Task<UserNotificationDto> AddUserNotification(UserNotificationDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/{dto.UserId}/notifications", dto);
            response.EnsureSuccessStatusCode();

            UserNotificationDto? createdNotification = await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            return createdNotification!;
        }
        
        public async Task<UserNotificationDto> UpdateUserNotification(UserNotificationDto dto)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{dto.UserId}/notifications/{dto.NotificationId}", dto);
            response.EnsureSuccessStatusCode();

            UserNotificationDto? updatedNotification = await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            return updatedNotification!;
        }
        
        public async Task<UserNotificationDto> DeleteUserNotification(int userId, int notificationId)
        {
            var response = await httpClient.DeleteAsync(
                $"{Controller}/{userId}/notifications/{notificationId}"
            );
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserNotificationDto>()
                   ?? throw new Exception("Error deleting notification");
        }
    }
}