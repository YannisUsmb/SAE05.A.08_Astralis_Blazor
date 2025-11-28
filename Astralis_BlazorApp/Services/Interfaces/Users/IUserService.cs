using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDetailDto> GetUserById(int id);
        Task<List<UserDetailDto>> GetAllUsers();
        Task<UserDetailDto> UpdateUser(int id, UserUpdateDto dto);
        Task<UserDetailDto> AddUser(UserCreateDto dto);
        Task<bool> ChangePassword(int id, ChangePasswordDto dto);
    }
}