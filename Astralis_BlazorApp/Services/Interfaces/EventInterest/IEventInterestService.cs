using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IEventInterestService
{
    Task<EventInterestDto?> GetByIdAsync(int eventId, int userId);
    Task<List<EventInterestDto>> GetAllAsync();
    Task<EventInterestDto?> AddAsync(EventInterestDto dto);
    Task<EventInterestDto?> DeleteAsync(int eventId, int userId);
}