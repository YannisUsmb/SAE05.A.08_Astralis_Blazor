using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ITypeOfArticleService
{
    Task<TypeOfArticleDto> GetByIdAsync(int articleTypeId, int articleId);
    Task<List<TypeOfArticleDto>> GetAllAsync();
    Task<TypeOfArticleDto> AddAsync(TypeOfArticleDto dto);
    Task<TypeOfArticleDto> UpdateAsync(int articleTypeId, int articleId, TypeOfArticleDto dto);
    Task<TypeOfArticleDto> DeleteAsync(int articleTypeId, int articleId);
}