using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICommandService
{
    Task<CommandDetailDto?> GetByIdAsync(int id);

    Task<List<CommandListDto>> GetByUserIdAsync(int userId);
    Task<List<CommandListDto>> GetByStatusIdAsync(int statusId);
    Task<CommandDetailDto?> CheckoutAsync(CommandCheckoutDto checkoutDto);
    Task UpdateStatusAsync(int id, CommandUpdateDto updateDto);
    Task<CommandDetailDto?> DeleteAsync(int id);
}