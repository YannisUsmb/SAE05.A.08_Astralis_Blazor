using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class ClientAiService(HttpClient httpClient) : IClientAiService
    {
        private const string Controller = "Ai";

        public async Task<PredictionResultDto?> PredictAsync(IBrowserFile file)
        {
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.Name);

            var response = await httpClient.PostAsync($"{Controller}/Predict", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PredictionResultDto>();
            }
            return null;
        }
    }
}