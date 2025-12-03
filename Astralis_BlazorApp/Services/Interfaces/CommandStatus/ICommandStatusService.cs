using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICommandStatusService
{
    Task<CommandStatusDto?> GetByIdAsync(int id);
    Task<List<CommandStatusDto>> GetAllAsync();
}