using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class ReportService(HttpClient httpClient) : IReportService
{
    private const string Controller = "Reports";
    
    public async Task<ReportDto?> GetByIdAsync(int id)
    {
        ReportDto? report = await httpClient.GetFromJsonAsync<ReportDto>($"{Controller}/{id}");
        return report ?? throw new Exception("Report not found");
    }
    
    public async Task<List<ReportDto>> GetAllAsync()
    {
        List<ReportDto>? report = await httpClient.GetFromJsonAsync<List<ReportDto>>(Controller);
        return report ?? new List<ReportDto>();
    }

    public async Task<ReportDto?> AddAsync(ReportCreateDto dto)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        ReportDto? createdReport = await response.Content.ReadFromJsonAsync<ReportDto>();
        return createdReport ?? throw new Exception("Error creating report");
    }

    public async Task<ReportDto?> UpdateAsync(int id, ReportUpdateDto dto)
    {
        if (id != dto.Id)
        {
            throw new Exception("ID mismatch between URL and Body");
        }

        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();

        ReportDto? updatedReport = await response.Content.ReadFromJsonAsync<ReportDto>();
        return updatedReport ?? throw new Exception("Error updating report");
    }

    public async Task<ReportDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();

        ReportDto? deletedReport = await response.Content.ReadFromJsonAsync<ReportDto>();
        return deletedReport ?? throw new Exception("Error deleting report");
    }

    public async Task<List<ReportDto>> GetByDateAsync(DateTime date)
    {
        string dateString = date.ToString("yyyy-MM-dd");
        
        List<ReportDto>? reports = await httpClient.GetFromJsonAsync<List<ReportDto>>($"{Controller}/date/{dateString}");
        return reports ?? new List<ReportDto>();
    }
    
    public async Task<List<ReportDto>> SearchAsync(ReportFilterDto filter)
    {
        string queryString = BuildSearchQueryString(filter);
        string url = $"{Controller}/search?{queryString}";

        List<ReportDto>? reports = await httpClient.GetFromJsonAsync<List<ReportDto>>(url);
        return reports ?? new List<ReportDto>();
    }

    private string BuildSearchQueryString(ReportFilterDto filter)
    {
        List<string> queryParams = new List<string>();

        if (filter.StatusId.HasValue)
            queryParams.Add($"statusId={filter.StatusId.Value}");

        if (filter.MotiveId.HasValue)
            queryParams.Add($"motiveId={filter.MotiveId.Value}");
        
        if (filter.MinDate.HasValue)
            queryParams.Add($"minDate={Uri.EscapeDataString(filter.MinDate.Value.ToString("O"))}");

        if (filter.MaxDate.HasValue)
            queryParams.Add($"maxDate={Uri.EscapeDataString(filter.MaxDate.Value.ToString("O"))}");

        return string.Join("&", queryParams);
    }
}