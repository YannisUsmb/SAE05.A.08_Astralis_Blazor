using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IReportStatusService
{
    Task<ReportStatusDto> GetByIdAsync(int id);
    Task<List<ReportStatusDto>> GetAllAsync();
}