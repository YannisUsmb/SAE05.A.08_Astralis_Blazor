using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDetailDto?> GetByIdAsync(int id);
        Task<List<UserDetailDto>> GetAllAsync();
        Task<UserDetailDto?> AddAsync(UserCreateDto dto);
        Task<UserDetailDto?> UpdateAsync(int id, UserUpdateDto dto);
        Task<UserDetailDto?> DeleteAsync(int id);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto);
    }
}