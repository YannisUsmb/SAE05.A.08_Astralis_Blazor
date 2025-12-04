using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IArticleTypeService
{
    Task<ArticleTypeDto?> GetByIdAsync(int id);
    Task<List<ArticleTypeDto>> GetAllAsync();
}