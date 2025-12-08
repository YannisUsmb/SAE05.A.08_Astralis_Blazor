using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class OrderDetailService(HttpClient httpClient) : IOrderDetailService
{
    private const string Controller = "OrderDetails";
    
    public async Task<OrderDetailDto> GetByIdAsync(int commandId, int productId)
    {
        return await httpClient.GetFromJsonAsync<OrderDetailDto>($"{Controller}/GetById?commandId={commandId}&productId={productId}") 
               ?? throw new Exception("OrderDetail not found");
    }
    
    public async Task<List<OrderDetailDto>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<OrderDetailDto>>($"{Controller}/GetAll") 
               ?? new List<OrderDetailDto>();
    }
    
    public async Task<OrderDetailDto?> AddAsync(OrderDetailCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        OrderDetailDto? createdDetail = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        return createdDetail ?? throw new Exception("Error adding product to order");
    }

    public async Task<OrderDetailDto?> UpdateAsync(int commandId, int productId, OrderDetailUpdateDto dto)
    {
        if (commandId != dto.CommandId || productId != dto.ProductId)
        {
            throw new Exception("ID mismatch between URL and Body");
        }

        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{commandId}/{productId}", dto);
        response.EnsureSuccessStatusCode();

        OrderDetailDto? updatedDetail = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        return updatedDetail ?? throw new Exception("Error updating order line");
    }

    public async Task<OrderDetailDto?> DeleteAsync(int commandId, int productId)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{commandId}/{productId}");
        response.EnsureSuccessStatusCode();

        OrderDetailDto? deletedDetail = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        return deletedDetail ?? throw new Exception("Error removing product from order");
    }
}