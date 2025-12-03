using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailDto> GetByIdAsync(int id);
    Task<List<ProductDetailDto>> GetAllAsync();
    Task<ProductDetailDto?> AddAsync(ProductCreateDto dto);
    Task<ProductDetailDto?> UpdateAsync(int id, ProductUpdateDto dto);
    Task<ProductDetailDto?> DeleteAsync(int id);
    Task<List<ProductListDto>> GetByNameAsync(string name);
    Task<List<ProductListDto>> GetByCategoryIdAsync(int categoryId);
}