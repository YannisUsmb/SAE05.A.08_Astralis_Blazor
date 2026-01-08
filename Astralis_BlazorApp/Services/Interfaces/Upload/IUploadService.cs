using Astralis.Shared.Enums;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(IBrowserFile file, UploadCategory category);
    }
}
