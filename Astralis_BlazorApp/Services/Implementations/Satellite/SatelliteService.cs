using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class SatelliteService(HttpClient httpClient) : ISatelliteService
{
    private const string Controller = "Satellites";

        public async Task<SatelliteDto?> GetByIdAsync(int id)
        {
            SatelliteDto? satellite = await httpClient.GetFromJsonAsync<SatelliteDto>($"{Controller}/{id}");
            return satellite ?? throw new Exception("Satellite not found");
        }

        public async Task<List<SatelliteDto>> GetAllAsync()
        {
            List<SatelliteDto>? satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(Controller);
            return satellites ?? new List<SatelliteDto>();
        }

        public async Task<SatelliteDto?> AddAsync(SatelliteCreateDto dto)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SatelliteDto>()
                   ?? throw new Exception("Error creating satellite");
        }

        public async Task<SatelliteDto> UpdateAsync(int id, SatelliteUpdateDto dto)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SatelliteDto>()
                   ?? throw new Exception("Error updating satellite");
        }

        public async Task<SatelliteDto?> DeleteAsync(int id)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SatelliteDto>()
                   ?? throw new Exception("Error deleting satellite");
        }

        public async Task<List<SatelliteDto>> GetByReferenceAsync(string reference)
        {
            List<SatelliteDto>? satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(
                $"{Controller}/reference/{reference}"
            );
            return satellites ?? new List<SatelliteDto>();
        }

        public async Task<List<SatelliteDto>> GetByPlanetIdAsync(int planetId)
        {
            List<SatelliteDto>? satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(
                $"{Controller}/planet/{planetId}"
            );
            return satellites ?? new List<SatelliteDto>();
        }
        
        public async Task<List<SatelliteDto>> SearchAsync(SatelliteFilterDto filter)
        {
            string queryString = BuildSearchQueryString(filter);
            string url = $"{Controller}/search?{queryString}";

            List<SatelliteDto>? satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(url);
            return satellites ?? new List<SatelliteDto>();
        }

        private string BuildSearchQueryString(SatelliteFilterDto filter)
        {
            List<string> queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                queryParams.Add($"name={Uri.EscapeDataString(filter.Name)}");

            if (filter.MinGravity.HasValue)
                queryParams.Add($"minGravity={filter.MinGravity.Value}");
            if (filter.MaxGravity.HasValue)
                queryParams.Add($"maxGravity={filter.MaxGravity.Value}");

            if (filter.MinRadius.HasValue)
                queryParams.Add($"minRadius={filter.MinRadius.Value}");
            if (filter.MaxRadius.HasValue)
                queryParams.Add($"maxRadius={filter.MaxRadius.Value}");

            if (filter.MinDensity.HasValue)
                queryParams.Add($"minDensity={filter.MinDensity.Value}");
            if (filter.MaxDensity.HasValue)
                queryParams.Add($"maxDensity={filter.MaxDensity.Value}");

            if (filter.PlanetIds is not { Count: > 0 }) return string.Join("&", queryParams);
            queryParams.AddRange(filter.PlanetIds.Select(id => $"planetIds={id}"));

            return string.Join("&", queryParams);
        }
}