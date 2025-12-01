using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserService(HttpClient httpClient) : IUserService
    {
        private const string Controller = "Users";

        public async Task<AuthResponseDto?> LoginAsync(int id, UserLoginDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/login/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return authResponse;
        }
        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            UserDetailDto? user = await httpClient.GetFromJsonAsync<UserDetailDto>($"{Controller}/{id}");
            return user;
        }
        
        public async Task<List<UserDetailDto>> GetAllAsync()
        {
            List<UserDetailDto>? users = await httpClient.GetFromJsonAsync<List<UserDetailDto>>($"{Controller}");

            if (users == null)
            {
                return new List<UserDetailDto>();
            }

            return users;
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