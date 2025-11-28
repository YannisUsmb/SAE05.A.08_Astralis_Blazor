using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string Controller = "users";
        
        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<AuthResponseDto?> Login(UserLoginDto dto)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{Controller}/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return authResponse;
        }
        public async Task<UserDetailDto?> GetUserById(int id)
        {
            UserDetailDto? user = await _httpClient.GetFromJsonAsync<UserDetailDto>($"{Controller}/{id}");
            return user;
        }
        
        public async Task<List<UserDetailDto>> GetAllUsers()
        {
            List<UserDetailDto>? users = await _httpClient.GetFromJsonAsync<List<UserDetailDto>>($"{Controller}");

            if (users == null)
            {
                return new List<UserDetailDto>();
            }

            return users;
        }

        public async Task<UserDetailDto?> AddUser( UserCreateDto dto)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{Controller}", dto);
            response.EnsureSuccessStatusCode();

            UserDetailDto? createdUser = await response.Content.ReadFromJsonAsync<UserDetailDto>();
            return createdUser;
        }

        public async Task<UserDetailDto?> UpdateUser(int id, UserUpdateDto dto)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{Controller}", dto);
            response.EnsureSuccessStatusCode();

            UserDetailDto? updatedUser = await response.Content.ReadFromJsonAsync<UserDetailDto>();
            return updatedUser;
        }

        public async Task<bool> ChangePassword(int id, ChangePasswordDto dto)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{Controller}/{id}/ChangePassword", dto);

            bool success = response.IsSuccessStatusCode;
            return success;
        }
    }
}