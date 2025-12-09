using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICartItemService
{
    Task<CartItemDto?> GetByIdAsync(int userId,int productId);
    Task<List<CartItemDto>> GetAllAsync();
    Task<CartDto?> GetMyCartAsync();
    Task<CartItemDto?> AddAsync(CartItemCreateDto dto);
    Task UpdateAsync(int userId,int productId, CartItemUpdateDto dto);
    Task DeleteAsync(int userId,int productId);
    Task ClearCartAsync(int userId);
}