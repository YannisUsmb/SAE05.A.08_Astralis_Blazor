using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class UserNotificationTypeService(HttpClient httpClient) : IUserNotificationTypeService
{
    private const string Controller = "UserNotificationTypes";
    
    public async Task<UserNotificationTypeDto?> GetByIdAsync(int userId, int notificationTypeId)
        {
            UserNotificationTypeDto? userNotificationType = await httpClient.GetFromJsonAsync<UserNotificationTypeDto>($"{Controller}/{userId}/{notificationTypeId}");
            return userNotificationType;
        }

        public async Task<List<UserNotificationTypeDto>> GetAllAsync()
        {
            List<UserNotificationTypeDto>? userNotificationTypes = await httpClient.GetFromJsonAsync<List<UserNotificationTypeDto>>(Controller);
            return userNotificationTypes ?? new List<UserNotificationTypeDto>();
        }

        public async Task<UserNotificationTypeDto?> AddAsync(UserNotificationTypeCreateDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);

            if (response.IsSuccessStatusCode)
            {
                UserNotificationTypeDto? createdUserNotificationType = await response.Content.ReadFromJsonAsync<UserNotificationTypeDto>();
                return createdUserNotificationType;
            }
            return null;
        }

        public async Task<UserNotificationTypeDto?> UpdateAsync(int userId, int notificationTypeId, UserNotificationTypeUpdateDto dto)
        {
            if (notificationTypeId != dto.NotificationTypeId)
            {
                throw new Exception("ID mismatch between URL and Body");
            }

            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{userId}/{notificationTypeId}", dto);

            if (response.IsSuccessStatusCode)
            {
                UserNotificationTypeDto? updatedUserNotificationType = await response.Content.ReadFromJsonAsync<UserNotificationTypeDto>();
                return updatedUserNotificationType;
            }
            return null;
        }

        public async Task<UserNotificationTypeDto?> DeleteAsync(int userId, int notificationTypeId)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{userId}/{notificationTypeId}");

            if (response.IsSuccessStatusCode)
            {
                UserNotificationTypeDto? deletedUserNotificationType = await response.Content.ReadFromJsonAsync<UserNotificationTypeDto>();
                return deletedUserNotificationType;
            }
            return null;
        }
}