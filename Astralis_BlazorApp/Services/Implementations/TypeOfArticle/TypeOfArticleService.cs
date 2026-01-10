using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class TypeOfArticleService(HttpClient httpClient) : ITypeOfArticleService
{
    private const string Controller = "TypesOfArticle";

    public async Task<TypeOfArticleDto> GetByIdAsync(int articleTypeId, int articleId)
    {
        string url = $"{Controller}/{articleTypeId}/{articleId}";
        
        TypeOfArticleDto? typeOfArticle = await httpClient.GetFromJsonAsync<TypeOfArticleDto>(url);
        return typeOfArticle ?? throw new Exception("Type of article not found");
    }

    public async Task<List<TypeOfArticleDto>> GetAllAsync()
    {
        List<TypeOfArticleDto>? typeOfArticles = await httpClient.GetFromJsonAsync<List<TypeOfArticleDto>>($"{Controller}");
        return typeOfArticles ?? new List<TypeOfArticleDto>();
    }

    public async Task<TypeOfArticleDto> AddAsync(TypeOfArticleDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        TypeOfArticleDto? createdTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return createdTypeOfArticle ?? throw new Exception("Failed to create type of article");
    }

    public async Task<TypeOfArticleDto> UpdateAsync(int articleTypeId, int articleId, TypeOfArticleDto dto)
    {
        string url = $"{Controller}/{articleTypeId}/{articleId}";
        
        HttpResponseMessage response = await httpClient.PutAsJsonAsync(url, dto);
        response.EnsureSuccessStatusCode();
        
        TypeOfArticleDto? updatedTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return updatedTypeOfArticle ?? throw new Exception("Failed to update type of article");
    }

    public async Task<TypeOfArticleDto> DeleteAsync(int articleTypeId, int articleId)
    {
        string url = $"{Controller}/{articleTypeId}/{articleId}";
        
        HttpResponseMessage response = await httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
        
        TypeOfArticleDto? deletedTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return deletedTypeOfArticle ?? throw new Exception("Failed to delete type of article");
    }
}