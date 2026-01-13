using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICityService
{
    Task<CityDto?> GetByIdAsync(int id);
    Task<List<CityDto>> GetAllAsync();
    Task<CityDto?> AddAsync(CityCreateDto dto);
    Task<List<CityDto>> SearchAsync(string term);
}