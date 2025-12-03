using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICountryService
{
    Task<CountryDto?> GetByIdAsync(int id);
    Task<List<CountryDto>> GetAllAsync();
}