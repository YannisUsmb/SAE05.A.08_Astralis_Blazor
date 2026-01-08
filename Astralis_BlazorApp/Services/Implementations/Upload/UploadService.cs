using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UploadService(HttpClient httpClient) : IUploadService
    {
        public async Task<string> UploadImageAsync(IBrowserFile file, UploadCategory category)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(5 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await httpClient.PostAsync($"Media/upload?category={category}", content);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UploadResult>();
            return result?.Url ?? string.Empty;
        }

        public class UploadResult { public string Url { get; set; } }
    }
}
