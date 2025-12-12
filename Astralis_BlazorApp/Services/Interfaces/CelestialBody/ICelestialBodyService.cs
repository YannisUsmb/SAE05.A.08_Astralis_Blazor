using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICelestialBodyService
{
    Task<List<CelestialBodyListDto>> GetAllAsync();
    Task<CelestialBodyListDto?> AddAsync(CelestialBodyCreateDto dto);
    Task<CelestialBodyListDto?> UpdateAsync(int id, CelestialBodyUpdateDto dto);
    Task<CelestialBodyListDto?> DeleteAsync(int id);
    Task<List<CelestialBodyListDto>> GetByNameAsync(string name);
    Task<List<CelestialBodyListDto>> SearchAsync(CelestialBodyFilterDto filter, int pageNumber, int pageSize);
}