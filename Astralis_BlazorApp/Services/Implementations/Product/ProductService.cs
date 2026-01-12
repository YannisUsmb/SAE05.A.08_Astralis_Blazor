using System.Net; // Ajout nécessaire pour HttpStatusCode
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

        // Ici, en général un POST renvoie un 201 avec du contenu, donc on lit.
        ProductDetailDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        return createdProduct ?? throw new Exception("Error creating product");
    }

    public async Task<ProductDetailDto?> UpdateAsync(int id, ProductUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        // --- CORRECTION ---
        // Si le serveur répond 204 (No Content), on ne lit pas le JSON pour éviter le crash.
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            // L'opération a réussi mais le serveur ne renvoie rien.
            // On retourne null (ou tu pourrais retourner un new ProductDetailDto() vide si tu préfères).
            return null;
        }

        ProductDetailDto? updatedProduct = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        return updatedProduct ?? throw new Exception("Error updating product");
    }

    public async Task<ProductDetailDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        // --- CORRECTION ---
        // Même chose pour le DELETE qui renvoie souvent 204
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

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

    public async Task<List<ProductListDto>> SearchAsync(ProductFilterDto filter)
    {
        string queryString = BuildSearchQueryString(filter);
        string url = $"{Controller}/search?{queryString}";

        List<ProductListDto>? products = await httpClient.GetFromJsonAsync<List<ProductListDto>>(url);
        return products ?? new List<ProductListDto>();
    }

    private string BuildSearchQueryString(ProductFilterDto filter)
    {
        List<string> queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            queryParams.Add($"searchText={Uri.EscapeDataString(filter.SearchText)}");

        if (filter.MinPrice.HasValue)
            queryParams.Add($"minPrice={filter.MinPrice.Value}");

        if (filter.MaxPrice.HasValue)
            queryParams.Add($"maxPrice={filter.MaxPrice.Value}");

        if (filter.ProductCategoryIds is not { Count: > 0 }) return string.Join("&", queryParams);
        queryParams.AddRange(filter.ProductCategoryIds.Select(id => $"productCategoryIds={id}"));

        return string.Join("&", queryParams);
    }
}