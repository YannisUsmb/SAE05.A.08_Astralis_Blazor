using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICometService
{
    Task<CometDto?> GetByIdAsync(int id);
    Task<List<CometDto>> GetAllAsync();
    Task<CometDto> AddAsync(CometCreateDto dto);
    Task<CometDto> UpdateAsync(int id, CometUpdateDto dto);
    Task<CometDto?> DeleteAsync(int id);
    Task<List<CometDto>> GetByReferenceAsync(string reference);
    Task<List<CometDto>> SearchAsync(CometFilterDto filter);
}