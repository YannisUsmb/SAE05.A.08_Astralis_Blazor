using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ScannerViewModel : ObservableObject
    {
        private readonly IClientAiService _aiService;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? imagePreviewUrl;

        [ObservableProperty]
        private PredictionResultDto? result;

        [ObservableProperty]
        private string? errorMessage;

        private IBrowserFile? _selectedFile;

        public ScannerViewModel(IClientAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task LoadImageAsync(InputFileChangeEventArgs e)
        {
            ErrorMessage = null;
            Result = null;
            _selectedFile = e.File;

            var format = "image/png";
            var resizedImage = await e.File.RequestImageFileAsync(format, 600, 600);
            var buffer = new byte[resizedImage.Size];
            await resizedImage.OpenReadStream().ReadAsync(buffer);
            ImagePreviewUrl = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
        }

        [RelayCommand]
        public async Task AnalyzeImageAsync()
        {
            if (_selectedFile == null) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Result = await _aiService.PredictAsync(_selectedFile);
                if (Result == null)
                {
                    ErrorMessage = "L'analyse a échoué. Veuillez réessayer.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur de connexion au scanner.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void Reset()
        {
            ImagePreviewUrl = null;
            Result = null;
            _selectedFile = null;
            ErrorMessage = null;
        }
    }
}