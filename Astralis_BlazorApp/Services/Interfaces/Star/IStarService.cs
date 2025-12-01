using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IStarService
{
    Task<StarDto> GetStarById(int id);
    Task<List<StarDto>> GetAllStars();
    Task<StarDto> AddStar(StarCreateDto dto);
    Task<StarDto> UpdateStar(int id, StarUpdateDto dto);
    Task<StarDto> DeleteStar(int id);
    Task<List<StarDto>>  GetByReference(string reference);
    Task<List<StarDto>> GetBySpectralClassId(int id);
}