using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserNotificationService(HttpClient httpClient) : IUserNotificationService
    {
        private const string Controller = "UserNotifications";

        public async Task<List<UserNotificationDto>> GetAllAsync(int userId)
        {
            var list = await httpClient.GetFromJsonAsync<List<UserNotificationDto>>($"{Controller}/{userId}/notifications");
            return list ?? new List<UserNotificationDto>();
        }

        public async Task<UserNotificationDto?> GetByIdAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<UserNotificationDto>($"{Controller}/{id}");
        }

        public async Task<UserNotificationDto?> AddAsync(UserNotificationCreateDto dto)
        {
            var response = await httpClient.PostAsJsonAsync(Controller, dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            }
            return null;
        }

        public async Task<UserNotificationDto?> UpdateAsync(int id, UserNotificationUpdateDto dto)
        {
            var response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
            }
            return null;
        }

        public async Task<UserNotificationDto?> DeleteAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"{Controller}/{id}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<UserNotificationDto>();
                }
                catch
                {
                    return new UserNotificationDto();
                }
            }
            return null;
        }
    }
}