using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IDiscoveryService
{
    Task<DiscoveryDto?> GetByIdAsync(int id);
    Task<List<DiscoveryDto>> GetAllAsync();
    Task<List<DiscoveryDto>> SearchAsync(DiscoveryFilterDto filter);
    Task<DiscoveryDto?> CreateAsteroidAsync(DiscoveryAsteroidSubmissionDto submission);
    Task<DiscoveryDto?> CreatePlanetAsync(DiscoveryPlanetSubmissionDto submission);
    Task<DiscoveryDto?> CreateStarAsync(DiscoveryStarSubmissionDto submission);
    Task<DiscoveryDto?> CreateCometAsync(DiscoveryCometSubmissionDto submission);
    Task<DiscoveryDto?> CreateGalaxyAsync(DiscoveryGalaxyQuasarSubmissionDto submission);
    Task<DiscoveryDto?> CreateSatelliteAsync(DiscoverySatelliteSubmissionDto submission);
    Task UpdateTitleAsync(int id, DiscoveryUpdateDto dto);
    Task ProposeAliasAsync(int id, DiscoveryAliasDto dto);
    Task ModerateStatusAsync(int id, DiscoveryModerationDto dto);
    Task<DiscoveryDto?> DeleteAsync(int id);
}