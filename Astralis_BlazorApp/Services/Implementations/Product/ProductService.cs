using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ProductService(HttpClient httpClient) : IProductService
{
    private const string Controller = "Products";
    
    public async Task<ProductDetailDto> GetByIdAsync(int id)
    {
        ProductDetailDto? product = await httpClient.GetFromJsonAsync<ProductDetailDto>($"{Controller}/{id}");
        return product ?? throw new Exception("Product not found");
    }
    
    public async Task<List<ProductDetailDto>> GetAllAsync()
    {
        List<ProductDetailDto>? products = await httpClient.GetFromJsonAsync<List<ProductDetailDto>>($"{Controller}");
        return products ?? new List<ProductDetailDto>();
    }
    
    public async Task<ProductDetailDto?> AddAsync(ProductCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        
        ProductDetailDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        return createdProduct ?? throw new Exception("Error creating product");
    }

    public async Task<ProductDetailDto?> UpdateAsync(int id, ProductUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        ProductDetailDto? updatedProduct = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        return updatedProduct ?? throw new Exception("Error updating product");
    }

    public async Task<ProductDetailDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        ProductDetailDto? deletedProduct = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        return deletedProduct ?? throw new Exception("Error deleting product");
    }
    
    public async Task<List<ProductListDto>> GetByNameAsync(string name)
    {
        List<ProductListDto>? products = await httpClient.GetFromJsonAsync<List<ProductListDto>>($"{Controller}/search/{name}");
        return products ?? new List<ProductListDto>();
    }

    public async Task<List<ProductListDto>> GetByCategoryIdAsync(int categoryId)
    {
        List<ProductListDto>? products = await httpClient.GetFromJsonAsync<List<ProductListDto>>($"{Controller}/category/{categoryId}");
        return products ?? new List<ProductListDto>();
    }
}