using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IStarService
{
    Task<StarDto> GetByIdAsync(int id);
    Task<List<StarDto>> GetAllAsync();
    Task<StarDto> AddAsync(StarCreateDto dto);
    Task<StarDto> UpdateAsync(int id, StarUpdateDto dto);
    Task<StarDto> DeleteAsync(int id);
    Task<List<StarDto>>  GetByReferenceAsync(string reference);
    Task<List<StarDto>> GetBySpectralClassIdAsync(int id);
    Task<List<StarDto>> SearchAsync(StarFilterDto filter);
}