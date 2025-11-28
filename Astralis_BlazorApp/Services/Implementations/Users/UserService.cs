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
        
        public async Task<UserDetailDto> GetUserById(int id)
        {
            return await _httpClient.GetFromJsonAsync<UserDetailDto>($"{Controller}/{id}") 
                   ?? new UserDetailDto();
        }
        
        public async Task<List<UserDetailDto>> GetAllUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDetailDto>>($"{Controller}") 
                   ?? new List<UserDetailDto>();
        }
        
        public async Task<UserDetailDto> UpdateUser(int id, UserUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDetailDto>() 
                   ?? new UserDetailDto();
        }
        
        public async Task<UserDetailDto> AddUser( UserCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{Controller}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDetailDto>() 
                   ?? new UserDetailDto();
        }
        
        public async Task<bool> ChangePassword(int id, ChangePasswordDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{Controller}/{id}/ChangePassword", dto);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
    }
}