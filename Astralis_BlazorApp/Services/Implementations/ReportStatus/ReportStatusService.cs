using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.ReportStatus;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.ReportStatus;

public class ReportStatusService(HttpClient httpClient) : IReportStatusService
{
    public const string Controller = "ReportStatus";
    
    public async Task<ReportStatusDto> GetByIdAsync(int id)
    {
        var reportStatus = await httpClient.GetFromJsonAsync<ReportStatusDto>($"{Controller}/{id}");
        return reportStatus ?? throw new Exception("Report status not found");
    }
    
    public async Task<List<ReportStatusDto>> GetAllAsync()
    {
        var reportStatuses = await httpClient.GetFromJsonAsync<List<ReportStatusDto>>(Controller);
        return reportStatuses ?? new List<ReportStatusDto>();
    }
}