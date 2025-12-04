using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.AliasStatus;

public interface IAliasStatusService
{
    Task<AliasStatusDto?> GetByIdAsync(int id);
    Task<List<AliasStatusDto>> GetAllAsync();
}