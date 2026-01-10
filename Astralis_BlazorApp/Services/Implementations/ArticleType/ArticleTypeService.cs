using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ArticleTypeService(HttpClient httpClient) : IArticleTypeService
{
    private const string Controller = "ArticleTypes";

    public async Task<ArticleTypeDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<ArticleTypeDto>($"{Controller}/{id}");
    }

    public async Task<List<ArticleTypeDto>> GetAllAsync()
    {
        var result = await httpClient.GetFromJsonAsync<List<ArticleTypeDto>>(Controller);
        return result ?? new List<ArticleTypeDto>();
    }

    public async Task<ArticleTypeDto> AddAsync(ArticleTypeDto dto)
    {
        var response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleTypeDto>()
               ?? throw new Exception("Erreur lors de la création du type.");
    }
}