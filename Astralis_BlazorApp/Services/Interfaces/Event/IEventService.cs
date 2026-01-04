using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDto?> GetByIdAsync(int id);
        Task<List<EventDto>> GetAllAsync();
        Task<EventDto?> AddAsync(EventCreateDto dto);
        Task UpdateAsync(int id, EventUpdateDto dto);
        Task DeleteAsync(int id);
        Task<List<EventDto>> SearchAsync(EventFilterDto filter, int pageNumber, int pageSize, string sortBy);
    }
}