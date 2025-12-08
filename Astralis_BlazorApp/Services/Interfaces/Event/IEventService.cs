using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IEventService
{
    Task<EventDto?> GetByIdAsync(int id);
    Task<List<EventDto>> GetAllAsync();
    Task<EventDto?> AddAsync(EventCreateDto eventDto);
    Task<EventDto?> UpdateAsync(int id, EventUpdateDto eventDto);
    Task<EventDto?> DeleteAsync(int id);
    Task<List<EventDto>> GetByTitleAsync(string title);
    Task<List<EventDto>> SearchAsync(EventFilterDto filter);
}