using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces;

public interface IReportService
{
    Task<ReportDto?> GetByIdAsync(int id);
    Task<List<ReportDto>> GetAllAsync();
    Task<ReportDto?> AddAsync(ReportCreateDto dto);
    Task<ReportDto?> UpdateAsync(int id, ReportUpdateDto dto);
    Task<ReportDto?> DeleteAsync(int id);
    Task<List<ReportDto>> GetByDateAsync(DateTime date);
}