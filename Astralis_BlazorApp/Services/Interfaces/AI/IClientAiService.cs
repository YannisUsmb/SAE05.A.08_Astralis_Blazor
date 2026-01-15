using Astralis.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IClientAiService
    {
        Task<PredictionResultDto?> PredictAsync(IBrowserFile file);
    }
}