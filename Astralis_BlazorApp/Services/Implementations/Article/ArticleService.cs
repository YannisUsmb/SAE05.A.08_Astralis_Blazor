using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ArticleService(HttpClient httpClient) : IArticleService
{
    private const string Controller = "Articles";

    public async Task<List<ArticleListDto>> GetAllAsync()
    {
        List<ArticleListDto>? result = await httpClient.GetFromJsonAsync<List<ArticleListDto>>(Controller);
        return result ?? new List<ArticleListDto>();
    }

    public async Task<ArticleDetailDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<ArticleDetailDto>($"{Controller}/{id}");
    }

    public async Task<ArticleDetailDto?> AddAsync(ArticleCreateDto createDto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, createDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ArticleDetailDto>();
    }

    public async Task UpdateAsync(int id, ArticleUpdateDto updateDto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", updateDto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ArticleListDto>> SearchAsync(ArticleFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<ArticleListDto>? result = await httpClient.GetFromJsonAsync<List<ArticleListDto>>(url);
        return result ?? new List<ArticleListDto>();
    }

    private string ToQueryString(ArticleFilterDto filter)
    {
        List<string> parameters = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            parameters.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
        }
        
        if (filter.IsPremium.HasValue)
        {
            parameters.Add($"isPremium={filter.IsPremium.Value}");
        }

        if (filter.TypeIds is { Count: > 0 })
        {
            parameters.AddRange(filter.TypeIds.Select(id => $"typeIds={id}"));
        }

        return string.Join("&", parameters);
    }
}