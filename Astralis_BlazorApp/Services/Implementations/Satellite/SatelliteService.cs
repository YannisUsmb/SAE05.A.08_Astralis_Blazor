using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces.Satellite;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations.Satellite;

public class SatelliteService(HttpClient httpClient) : ISatelliteService
{
    private const string Controller = "Satellites";

        public async Task<SatelliteDto> GetByIdAsync(int id)
        {
            var satellite = await httpClient.GetFromJsonAsync<SatelliteDto>($"{Controller}/{id}");
            return satellite ?? throw new Exception("Satellite not found");
        }

        public async Task<List<SatelliteDto>> GetAllAsync()
        {
            var satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(Controller);
            return satellites ?? new List<SatelliteDto>();
        }

        public async Task<SatelliteDto> AddAsync(SatelliteCreateDto dto)
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

        public async Task<SatelliteDto> DeleteAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"{Controller}/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SatelliteDto>()
                   ?? throw new Exception("Error deleting satellite");
        }

        public async Task<List<SatelliteDto>> GetByReferenceAsync(string reference)
        {
            var satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(
                $"{Controller}/reference/{reference}"
            );
            return satellites ?? new List<SatelliteDto>();
        }

        public async Task<List<SatelliteDto>> GetByPlanetIdAsync(int planetId)
        {
            var satellites = await httpClient.GetFromJsonAsync<List<SatelliteDto>>(
                $"{Controller}/planet/{planetId}"
            );
            return satellites ?? new List<SatelliteDto>();
        }
}