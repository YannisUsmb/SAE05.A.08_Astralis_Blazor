using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportCreateDto> CreateReport(ReportCreateDto dto);
        Task<ReportUpdateDto> UpdateReport(ReportUpdateDto dto);
    }
}