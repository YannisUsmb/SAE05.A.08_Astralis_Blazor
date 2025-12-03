using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ReportMotiveService(HttpClient httpClient) : IReportMotiveService
{
    private const string Controller = "ReportMotives";
    
    public async Task<ReportMotiveDto> GetByIdAsync(int id)
    {
        ReportMotiveDto? reportMotive = await httpClient.GetFromJsonAsync<ReportMotiveDto>($"{Controller}/{id}");
        return reportMotive ?? throw new Exception("Report motive not found");
    }

    public async Task<List<ReportMotiveDto>> GetAllAsync()
    {
        List<ReportMotiveDto>? reportMotives = await httpClient.GetFromJsonAsync<List<ReportMotiveDto>>(Controller);
        return reportMotives ?? new List<ReportMotiveDto>();
    }
}