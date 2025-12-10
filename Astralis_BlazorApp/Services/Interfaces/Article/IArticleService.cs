using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IArticleService
{
    Task<List<ArticleListDto>> GetAllAsync();
    Task<ArticleDetailDto?> GetByIdAsync(int id);   
    Task<List<ArticleListDto>> SearchAsync(ArticleFilterDto filter);
    Task<ArticleDetailDto?> AddAsync(ArticleCreateDto createDto);
    Task UpdateAsync(int id, ArticleUpdateDto updateDto);
    Task DeleteAsync(int id);
}