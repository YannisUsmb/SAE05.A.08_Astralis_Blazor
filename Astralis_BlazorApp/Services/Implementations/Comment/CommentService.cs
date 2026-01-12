using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CommentService(HttpClient httpClient) : ICommentService
{
    private const string Controller = "Comments";
    
    public async Task<CommentDto?> GetByIdAsync(int id)
    {
        CommentDto? comment = await httpClient.GetFromJsonAsync<CommentDto>($"{Controller}/{id}");
        return comment ?? throw new Exception("Comment not found");
    }

    public async Task<List<CommentDto>> GetByArticleIdAsync(int articleId)
    {
        var comments = await httpClient.GetFromJsonAsync<List<CommentDto>>($"{Controller}/Article/{articleId}?t={DateTime.UtcNow.Ticks}");
        return comments ?? new List<CommentDto>();
    }

    public async Task<List<CommentDto>> GetAllAsync()
    {
        List<CommentDto>? comments = await httpClient.GetFromJsonAsync<List<CommentDto>>($"{Controller}");
        return comments ?? new List<CommentDto>();
    }

    public async Task<CommentDto?> AddAsync(CommentCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();
        CommentDto? createdComment = await response.Content.ReadFromJsonAsync<CommentDto>();
        return createdComment ?? throw new Exception("Unable to add comment");
    }
    
    public async Task<CommentDto?> UpdateAsync(int id, CommentUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        CommentDto? updatedComment = await response.Content.ReadFromJsonAsync<CommentDto>();
        return updatedComment ?? throw new Exception("Unable to update comment");
    }
    
    public async Task<CommentDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
        CommentDto? deletedComment = await response.Content.ReadFromJsonAsync<CommentDto>();
        return deletedComment ?? throw new Exception("Unable to delete comment");
    }
}