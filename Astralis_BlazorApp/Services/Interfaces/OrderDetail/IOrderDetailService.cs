using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IOrderDetailService
{
    Task<OrderDetailDto> GetByIdAsync(int commandId, int productId);
    Task<List<OrderDetailDto>> GetAllAsync();
    Task<OrderDetailDto?> AddAsync(OrderDetailCreateDto dto);
    Task<OrderDetailDto?> UpdateAsync(int commandId, int productId, OrderDetailUpdateDto dto);
    Task<OrderDetailDto?> DeleteAsync(int commandId, int productId);
}