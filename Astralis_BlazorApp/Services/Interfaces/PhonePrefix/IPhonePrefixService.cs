using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.PhonePrefix;

public interface IPhonePrefixService
{
    Task<PhonePrefixDto> GetByIdAsync(int id);
    Task<List<PhonePrefixDto>> GetAllAsync();
}