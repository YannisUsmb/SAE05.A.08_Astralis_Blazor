using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IAddressService
{
    Task<AddressDto?> GetByIdAsync(int id);
    Task<List<AddressDto>> GetAllAsync();
    Task<AddressDto?> AddAsync(AddressCreateDto addressCreateDto);
    Task<AddressDto?> UpdateAsync(int id, AddressUpdateDto addressUpdateDto);
    Task<AddressDto?> DeleteAsync(int id);
}