using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserNotificationService(HttpClient httpClient) : IUserNotificationService
    {
        private const string Controller = "UserNotifications";
        
        public async Task<UserNotificationDto?> GetByIdAsync(int userId, int notificationId)
        {
            UserNotificationDto? notification = await httpClient.GetFromJsonAsync<UserNotificationDto>($"{Controller}/{userId}/notifications/{notificationId}");
            return notification!;
        }
        
        public async Task<List<UserNotificationDto>> GetAllAsync(int userId)
        {
            List<UserNotificationDto>? notifications = await httpClient.GetFromJsonAsync<List<UserNotificationDto>>($"{Controller}/{userId}/notifications");
            return notifications ?? new List<UserNotificationDto>();
        }
        
        public async Task<UserNotificationDto?> AddAsync(UserNotificationCreateDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/{dto.UserId}/notifications", dto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            }
            return null;
        }
        
        public async Task<UserNotificationDto?> UpdateAsync(int userId, int notificationId, UserNotificationUpdateDto dto)
        {
            if (userId != dto.UserId || notificationId != dto.NotificationId)
            {
                throw new Exception("ID mismatch between URL and Body");
            }

            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{userId}/notifications/{notificationId}", dto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            }
            return null;
        }
        
        public async Task<UserNotificationDto?> DeleteAsync(int userId, int notificationId)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{userId}/notifications/{notificationId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            }
            return null;
        }
    }
}