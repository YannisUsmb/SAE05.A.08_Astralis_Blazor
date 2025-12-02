using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.ProductCategory;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.ProductCategory;

public class ProductCategoryService(HttpClient httpClient) : IProductCategoryService
{
    private const string Controller = "ProductCategory";
    
    public async Task<ProductCategoryDto?> GetByIdAsync(int id)
    {
        ProductCategoryDto? productCategory = await httpClient.GetFromJsonAsync<ProductCategoryDto>($"{Controller}/{id}");
        return productCategory;
    }
    
    public async Task<List<ProductCategoryDto>> GetAllAsync()
    {
        List<ProductCategoryDto>? productCategories = await httpClient.GetFromJsonAsync<List<ProductCategoryDto>>(Controller);
        return productCategories ?? new List<ProductCategoryDto>();
    }
}