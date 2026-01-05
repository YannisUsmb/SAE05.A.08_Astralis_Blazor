using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class UploadService(HttpClient httpClient) : IUploadService
    {
        private const string Controller = "Upload";

        public async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            long maxFileSize = 1024 * 1024 * 5;

            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.Name);

                var response = await httpClient.PostAsync($"{Controller}", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResultDto>();
                    return result?.Url;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur upload : {ex.Message}");
            }

            return null;
        }
    }
}
