using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ArticleTypeService(HttpClient httpClient) : IArticleTypeService
{
    private const string Controller = "ArticleTypes";
    
    public async Task<ArticleTypeDto?> GetByIdAsync(int id)
    {
        ArticleTypeDto? articleType = await httpClient.GetFromJsonAsync<ArticleTypeDto>($"{Controller}/{id}");
        return articleType;
    }
    
    public async Task<List<ArticleTypeDto>> GetAllAsync()
    {
        List<ArticleTypeDto>? articleTypes = await httpClient.GetFromJsonAsync<List<ArticleTypeDto>>(Controller);
        return articleTypes ?? new List<ArticleTypeDto>();
    }
}