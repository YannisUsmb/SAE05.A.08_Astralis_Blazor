using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CommandStatusService(HttpClient httpClient) : ICommandStatusService
{
    private const string Controller = "CommandStatuses";
    
    public async Task<CommandStatusDto?> GetByIdAsync(int id)
    {
        CommandStatusDto? commandStatus = await httpClient.GetFromJsonAsync<CommandStatusDto>($"{Controller}/{id}");
        return commandStatus ?? throw new Exception("Command status not found");
    }
    
    public async Task<List<CommandStatusDto>> GetAllAsync()
    {
        List<CommandStatusDto>? commandStatuses = await httpClient.GetFromJsonAsync<List<CommandStatusDto>>(Controller);
        return commandStatuses ?? new List<CommandStatusDto>();
    }
}