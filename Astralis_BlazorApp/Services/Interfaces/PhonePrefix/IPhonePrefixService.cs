using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IPhonePrefixService
{
    Task<PhonePrefixDto> GetByIdAsync(int id);
    Task<List<PhonePrefixDto>> GetAllAsync();
}