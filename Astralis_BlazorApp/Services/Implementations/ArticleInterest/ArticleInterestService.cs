using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ArticleInterestService(HttpClient httpClient) : IArticleInterestService
{
    private const string Controller = "ArticleInterests";

    public async Task<List<ArticleInterestDto>> GetAllAsync()
    {
        List<ArticleInterestDto>? result = await httpClient.GetFromJsonAsync<List<ArticleInterestDto>>(Controller);
        return result ?? new List<ArticleInterestDto>();
    }

    public async Task<ArticleInterestDto?> GetByIdAsync(int articleId, int userId)
    {
        try 
        {
            return await httpClient.GetFromJsonAsync<ArticleInterestDto>($"{Controller}/{articleId}/{userId}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddAsync(ArticleInterestDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int articleId, int userId)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{articleId}/{userId}");
        response.EnsureSuccessStatusCode();
    }
}