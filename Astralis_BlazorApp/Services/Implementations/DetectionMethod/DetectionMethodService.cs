using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class DetectionMethodService(HttpClient httpClient) : IDetectionMethodService
{
    private const string Controller = "DetectionMethods";
    
    public async Task<DetectionMethodDto?> GetByIdAsync(int id)
    {
        DetectionMethodDto? detectionMethod = await httpClient.GetFromJsonAsync<DetectionMethodDto>($"{Controller}/{id}");
        return detectionMethod ?? throw new Exception("Detection Method not found");
    }
    
    public async Task<List<DetectionMethodDto>> GetAllAsync()
    {
        List<DetectionMethodDto>? detectionMethods = await httpClient.GetFromJsonAsync<List<DetectionMethodDto>>($"{Controller}");
        return detectionMethods ?? new List<DetectionMethodDto>();
    }
}