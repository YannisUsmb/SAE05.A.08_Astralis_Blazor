using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IProductCategoryService
{
    Task<ProductCategoryDto?> GetByIdAsync(int id);
    Task<List<ProductCategoryDto>> GetAllAsync();
}