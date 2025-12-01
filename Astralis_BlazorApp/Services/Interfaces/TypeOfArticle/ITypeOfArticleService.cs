using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ITypeOfArticleService
{
    Task<TypeOfArticleDto> GetTypeOfArticleById(int id);
    Task<List<TypeOfArticleDto>> GetAllTypeOfArticles();
    Task<TypeOfArticleDto> AddTypeOfArticle(TypeOfArticleDto dto);
    Task<TypeOfArticleDto> UpdateTypeOfArticle(int id, TypeOfArticleDto dto);
    Task<TypeOfArticleDto> DeleteTypeOfArticle(int id);
}