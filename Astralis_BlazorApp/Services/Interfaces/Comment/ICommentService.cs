using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface ICommentService
{
    Task<CommentDto?> GetByIdAsync(int id);
    Task<List<CommentDto>> GetAllAsync();
    Task<CommentDto?> AddAsync(CommentCreateDto commentCreateDto);
    Task<CommentDto?> UpdateAsync(int id, CommentUpdateDto commentUpdateDto);
    Task<CommentDto?> DeleteAsync(int id);
}