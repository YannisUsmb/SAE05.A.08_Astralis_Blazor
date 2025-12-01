using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ITypeOfArticleService
{
    Task<TypeOfArticleDto> GetByIdAsync(int id);
    Task<List<TypeOfArticleDto>> GetAllAsync();
    Task<TypeOfArticleDto> AddAsync(TypeOfArticleDto dto);
    Task<TypeOfArticleDto> UpdateAsync(int id, TypeOfArticleDto dto);
    Task<TypeOfArticleDto> DeleteAsync(int id);
    
    // A finir plus tard TABLE JOINTURE
}