using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces.ReportMotive;

public interface IReportMotiveService
{
    Task<ReportMotiveDto> GetByIdAsync(int id);
    Task<List<ReportMotiveDto>> GetAllAsync();
}