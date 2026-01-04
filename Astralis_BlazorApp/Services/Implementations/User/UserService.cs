using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserService(HttpClient httpClient) : IUserService
    {
        private const string Controller = "Users";

        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<UserDetailDto>($"{Controller}/{id}");
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                Console.WriteLine($"ERREUR API GetById: {ex.StatusCode} - {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserDetailDto>> GetAllAsync()
        {
            List<UserDetailDto>? users = await httpClient.GetFromJsonAsync<List<UserDetailDto>>($"{Controller}");
            return users ?? new List<UserDetailDto>();
        }

        public async Task<UserDetailDto?> AddAsync(UserCreateDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}", dto);
            response.EnsureSuccessStatusCode();

            UserDetailDto? createdUser = await response.Content.ReadFromJsonAsync<UserDetailDto>();
            return createdUser;
        }

        public async Task<UserDetailDto?> UpdateAsync(int id, UserUpdateDto dto)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            UserDetailDto? updatedUser = await response.Content.ReadFromJsonAsync<UserDetailDto>();
            return updatedUser;
        }

        public async Task<UserDetailDto?> DeleteAsync(int id)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            UserDetailDto? deletedUser = await response.Content.ReadFromJsonAsync<UserDetailDto>();
            return deletedUser;
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/ChangePassword", dto);

            bool success = response.IsSuccessStatusCode;
            return success;
        }
    }
}