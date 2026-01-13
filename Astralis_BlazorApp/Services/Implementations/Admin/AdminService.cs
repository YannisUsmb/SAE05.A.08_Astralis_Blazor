using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class AdminService(HttpClient httpClient) : IAdminService
    {
        private const string Controller = "Admin";

        public async Task<List<DiscoveryDto>> GetPendingDiscoveriesAsync()
        {
            // GET api/Admin/Discoveries/Pending
            return await httpClient.GetFromJsonAsync<List<DiscoveryDto>>($"{Controller}/Discoveries/Pending") 
                   ?? new List<DiscoveryDto>();
        }

        public async Task ApproveDiscoveryAsync(int id)
        {
            // POST api/Admin/Discoveries/{id}/Approve
            var response = await httpClient.PostAsync($"{Controller}/Discoveries/{id}/Approve", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RejectDiscoveryAsync(int id, string reason)
        {
            // POST api/Admin/Discoveries/{id}/Reject
            var dto = new DiscoveryRejectionDto { Reason = reason };
            var response = await httpClient.PostAsJsonAsync($"{Controller}/Discoveries/{id}/Reject", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}