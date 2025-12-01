using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UserRoleService(HttpClient httpClient) : IUserRoleService
    {
        private const string Controller = "UserRoles";
        
        public async Task<UserRoleDto> GetUserRoleById(int id)
        {
            UserRoleDto? role = await httpClient.GetFromJsonAsync<UserRoleDto>($"{Controller}/{id}");
            return role ?? throw new Exception("Role not found");
        }
        
        public async Task<List<UserRoleDto>> GetAllUserRoles()
        {
            List<UserRoleDto>? roles = await httpClient.GetFromJsonAsync<List<UserRoleDto>>($"{Controller}");

            if (roles == null)
            {
                return new List<UserRoleDto>();
            }

            return roles;
        }
    }
}