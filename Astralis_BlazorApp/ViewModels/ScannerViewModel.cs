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

            try
            {
                var format = "image/png";
                var resizedImage = await e.File.RequestImageFileAsync(format, 600, 600);

                var buffer = new byte[resizedImage.Size];

                await resizedImage.OpenReadStream(10 * 1024 * 1024).ReadAsync(buffer);

                ImagePreviewUrl = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur lors du chargement de l'image. Fichier trop volumineux ?";
                Console.WriteLine(ex.ToString());
            }
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
                    ErrorMessage = "L'IA n'a renvoyé aucun résultat.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                ErrorMessage = httpEx.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ErrorMessage = "Une erreur inattendue s'est produite. Vérifiez la console.";
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