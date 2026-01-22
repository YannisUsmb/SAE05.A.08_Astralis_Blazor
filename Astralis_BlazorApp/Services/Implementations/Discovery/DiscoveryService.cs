using System.Net;
using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class DiscoveryService(HttpClient httpClient) : IDiscoveryService
{
    private const string Controller = "Discoveries"; 

    public async Task<DiscoveryDto?> GetByIdAsync(int id)
    {
        var response = await httpClient.GetAsync($"{Controller}/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    public async Task<List<DiscoveryDto>> GetAllAsync()
    {
        try 
        {
            return await httpClient.GetFromJsonAsync<List<DiscoveryDto>>(Controller) ?? new List<DiscoveryDto>();
        }
        catch 
        {
            return new List<DiscoveryDto>();
        }
    }

    // --- CREATE ---

    public async Task<DiscoveryDto?> CreateAsteroidAsync(DiscoveryAsteroidSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Asteroid", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    public async Task<DiscoveryDto?> CreatePlanetAsync(DiscoveryPlanetSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Planet", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    public async Task<DiscoveryDto?> CreateStarAsync(DiscoveryStarSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Star", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    public async Task<DiscoveryDto?> CreateCometAsync(DiscoveryCometSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Comet", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    public async Task<DiscoveryDto?> CreateGalaxyAsync(DiscoveryGalaxyQuasarSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/GalaxyQuasar", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }
    
    public async Task<DiscoveryDto?> CreateSatelliteAsync(DiscoverySatelliteSubmissionDto submission)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{Controller}/Satellite", submission);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
    }

    // --- UPDATES ---

    public async Task UpdateTitleAsync(int id, DiscoveryUpdateDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}", dto);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<bool> ProposeAliasAsync(int id, DiscoveryAliasDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/Alias", dto);
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.PaymentRequired)
            {
                throw new HttpRequestException("Paiement requis pour cette action", null, HttpStatusCode.PaymentRequired);
            }
            
            response.EnsureSuccessStatusCode();
        }

        return true;
    }
    
    public async Task<bool> RemoveAliasAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}/Alias");
        response.EnsureSuccessStatusCode();
        return true;
    }
    
    public async Task<bool> ModerateAliasAsync(int id, DiscoveryModerationDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/Alias/Moderate", dto);
        response.EnsureSuccessStatusCode(); 
        return true;
    }

    public async Task ModerateStatusAsync(int id, DiscoveryModerationDto dto)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{id}/Status", dto);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<string> CreateAliasPaymentSessionAsync(int discoveryId, string aliasProposed)
    {
        var payload = new { Alias = aliasProposed };
        var response = await httpClient.PostAsJsonAsync($"{Controller}/{discoveryId}/Alias/Checkout", payload);
    
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<CheckoutResponseDto>();
        return result?.Url ?? "";
    }
    
    public async Task ValidateAliasPaymentAsync(string sessionId)
    {
        var response = await httpClient.PostAsync($"{Controller}/Alias/ValidatePayment?sessionId={sessionId}", null);
        response.EnsureSuccessStatusCode();
    }
    
    public class CheckoutResponseDto
    {
        public string Url { get; set; } = string.Empty;
    }

    public async Task<DiscoveryDto?> DeleteAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{id}");
        response.EnsureSuccessStatusCode();
        
        try 
        {
            return await response.Content.ReadFromJsonAsync<DiscoveryDto>();
        }
        catch 
        {
            return null; 
        }
    }

    public async Task<List<DiscoveryDto>> SearchAsync(DiscoveryFilterDto filter)
    {
        string queryString = ToQueryString(filter);
        string url = $"{Controller}/Search?{queryString}";

        List<DiscoveryDto>? discoveries = await httpClient.GetFromJsonAsync<List<DiscoveryDto>>(url);
        return discoveries ?? new List<DiscoveryDto>();
    }

    // --- HELPER ---

    private string ToQueryString(DiscoveryFilterDto filter)
    {
        List<string> parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Title))
            parameters.Add($"title={Uri.EscapeDataString(filter.Title)}");

        if (filter.DiscoveryStatusId.HasValue)
            parameters.Add($"discoveryStatusId={filter.DiscoveryStatusId}");

        if (filter.AliasStatusId.HasValue)
            parameters.Add($"aliasStatusId={filter.AliasStatusId}");

        if (filter.DiscoveryApprovalUserId.HasValue)
            parameters.Add($"discoveryApprovalUserId={filter.DiscoveryApprovalUserId}");

        if (filter.AliasApprovalUserId.HasValue)
            parameters.Add($"aliasApprovalUserId={filter.AliasApprovalUserId}");

        return string.Join("&", parameters);
    }
}