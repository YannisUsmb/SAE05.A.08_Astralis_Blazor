using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CommandService(HttpClient httpClient) : ICommandService
{
    private const string Controller = "Commands";
    
    public async Task<CommandDetailDto?> GetByIdAsync(int id)
    {
        CommandDetailDto? command = await httpClient.GetFromJsonAsync<CommandDetailDto>($"{Controller}/{id}");
        return command ?? throw new Exception("Command not found");
    }

    public async Task<List<CommandListDto>> GetByUserIdAsync(int userId)
    {
        List<CommandListDto>? commands = await httpClient.GetFromJsonAsync<List<CommandListDto>>($"{Controller}/User/{userId}");
        return commands ?? new List<CommandListDto>();
    }

    public async Task<List<CommandListDto>> GetByStatusIdAsync(int statusId)
    {
        List<CommandListDto>? commands = await httpClient.GetFromJsonAsync<List<CommandListDto>>($"{Controller}/Status/{statusId}");
        return commands ?? new List<CommandListDto>();
    }

    public async Task<CommandDetailDto?> CheckoutAsync(CommandCheckoutDto checkoutDto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Checkout", checkoutDto);
        response.EnsureSuccessStatusCode();

        CommandDetailDto? createdCommand = await response.Content.ReadFromJsonAsync<CommandDetailDto>();
        return createdCommand ?? throw new Exception("Error during checkout process");
    }

    public async Task UpdateStatusAsync(int id, CommandUpdateDto updateDto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", updateDto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CommandDetailDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<CommandDetailDto>();
        }
        catch
        {
            return null;
        }
    }
}