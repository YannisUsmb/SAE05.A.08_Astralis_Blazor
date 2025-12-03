using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ReportStatusService(HttpClient httpClient) : IReportStatusService
{
    private const string Controller = "ReportStatuses";
    
    public async Task<ReportStatusDto> GetByIdAsync(int id)
    {
        ReportStatusDto? reportStatus = await httpClient.GetFromJsonAsync<ReportStatusDto>($"{Controller}/{id}");
        return reportStatus ?? throw new Exception("Report status not found");
    }
    
    public async Task<List<ReportStatusDto>> GetAllAsync()
    {
        List<ReportStatusDto>? reportStatuses = await httpClient.GetFromJsonAsync<List<ReportStatusDto>>(Controller);
        return reportStatuses ?? new List<ReportStatusDto>();
    }
}