using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserRoleService
{
        Task<UserRoleDto> GetUserRoleById(int id);
        Task<List<UserRoleDto>> GetAllUserRoles();
}