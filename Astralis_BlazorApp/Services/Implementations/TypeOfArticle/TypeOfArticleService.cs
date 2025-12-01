using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.TypeOfArticle;

public class TypeOfArticleService(HttpClient httpClient) : ITypeOfArticleService
{
    private const string Controller = "TypeOfArticles";

    public async Task<TypeOfArticleDto> GetTypeOfArticleById(int id)
    {
        TypeOfArticleDto? typeOfArticle = await httpClient.GetFromJsonAsync<TypeOfArticleDto>($"{Controller}/{id}");
        return typeOfArticle ?? throw new Exception("Type of article not found");
    }

    public async Task<List<TypeOfArticleDto>> GetAllTypeOfArticles()
    {
        List<TypeOfArticleDto>? typeOfArticles = await httpClient.GetFromJsonAsync<List<TypeOfArticleDto>>($"{Controller}");

        if (typeOfArticles == null)
        {
            return new List<TypeOfArticleDto>();
        }

        return typeOfArticles;
    }

    public async Task<TypeOfArticleDto> AddTypeOfArticle(TypeOfArticleDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        TypeOfArticleDto? createdTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return createdTypeOfArticle ?? throw new Exception("Failed to create type of article");
    }

    public async Task<TypeOfArticleDto> UpdateTypeOfArticle(int id, TypeOfArticleDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();
        TypeOfArticleDto? updatedTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return updatedTypeOfArticle ?? throw new Exception("Failed to update type of article");
    }

    public async Task<TypeOfArticleDto> DeleteTypeOfArticle(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
        TypeOfArticleDto? deletedTypeOfArticle = await response.Content.ReadFromJsonAsync<TypeOfArticleDto>();
        return deletedTypeOfArticle ?? throw new Exception("Failed to delete type of article");
    }
    
}