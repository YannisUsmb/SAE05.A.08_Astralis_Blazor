using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class AddressService(HttpClient httpClient) : IAddressService
{
    private const string Controller = "Addresses";

    public async Task<AddressDto?> GetByIdAsync(int id)
    {
        AddressDto? address = await httpClient.GetFromJsonAsync<AddressDto>($"{Controller}/{id}");
        return address;
    }
    
    public async Task<List<AddressDto>> GetAllAsync()
    {
        List<AddressDto>? addresses = await httpClient.GetFromJsonAsync<List<AddressDto>>(Controller);
        return addresses ?? new List<AddressDto>();
    }

    public async Task<AddressDto?> AddAsync(AddressCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        AddressDto? createdAddress = await response.Content.ReadFromJsonAsync<AddressDto>();
        return createdAddress ?? throw new Exception("Unable to add address");
    }
    
    public async Task<AddressDto?> UpdateAsync(int id, AddressUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        AddressDto? updatedAddress = await response.Content.ReadFromJsonAsync<AddressDto>();
        return updatedAddress ?? throw new Exception("Unable to update address");
    }
    
    public async Task<AddressDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
        AddressDto? deletedAddress = await response.Content.ReadFromJsonAsync<AddressDto>();
        return deletedAddress ?? throw new Exception("Unable to delete address");
    }
}