using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.EventType;

public interface IEventTypeService
{
    Task<EventTypeDto?> GetByIdAsync(int id);
    Task<List<EventTypeDto>> GetAllAsync();
}