using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IUserRoleService
{
        Task<UserRoleDto> GetByIdAsync(int id);
        Task<List<UserRoleDto>> GetAllAsync();
}