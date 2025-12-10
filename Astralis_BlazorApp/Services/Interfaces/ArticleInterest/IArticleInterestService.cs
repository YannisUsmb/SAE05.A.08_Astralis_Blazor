using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IArticleInterestService
{
    Task<List<ArticleInterestDto>> GetAllAsync();
    Task<ArticleInterestDto?> GetByIdAsync(int articleId, int userId);
    Task AddAsync(ArticleInterestDto dto);
    Task DeleteAsync(int articleId, int userId);
}