using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ISatelliteService
{
    Task<SatelliteDto> GetByIdAsync(int id);
    Task<List<SatelliteDto>> GetAllAsync();
    Task<SatelliteDto> AddAsync(SatelliteCreateDto dto);
    Task<SatelliteDto> UpdateAsync(int id, SatelliteUpdateDto dto);
    Task<SatelliteDto> DeleteAsync(int id);
    Task<List<SatelliteDto>> GetByReferenceAsync(string reference);
    Task<List<SatelliteDto>> GetByPlanetIdAsync(int planetId);
}