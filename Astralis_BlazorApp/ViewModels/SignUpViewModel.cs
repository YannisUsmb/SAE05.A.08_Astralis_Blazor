using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICountryService _countryService;
        private readonly IUploadService _uploadService;

        private readonly NavigationManager _navigation;

        [ObservableProperty]
        private UserCreateDto registerData = new()
        {
            Gender = GenderType.Male,
            MultiFactorAuthentification = false
        };

        [ObservableProperty]
        private ObservableCollection<CountryDto> countries = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public bool IsUploading { get; private set; } = false;

        public SignUpViewModel(IUserService userService, ICountryService countryService, IUploadService uploadService, NavigationManager navigation)
        {
            _userService = userService;
            _countryService = countryService;
            _navigation = navigation;
            _uploadService = uploadService;
        }

        public async Task LoadCountriesAsync()
        {
            try
            {
                var list = await _countryService.GetAllAsync();
                var sortedList = list.OrderBy(c => c.Name).ToList();
                Countries = new ObservableCollection<CountryDto>(sortedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement pays: {ex.Message}");
            }
        }

        public async Task UploadAvatarAsync(IBrowserFile file)
        {
            if (file == null) return;

            IsUploading = true;

            try
            {
                var url = await _uploadService.UploadImageAsync(file);

                if (!string.IsNullOrEmpty(url))
                {
                    RegisterData.AvatarUrl = url;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Échec de l'envoi de l'image.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur technique lors de l'upload.";
            }
            finally
            {
                IsUploading = false;
            }
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (isLoading) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                if (RegisterData.Password != RegisterData.ConfirmPassword)
                {
                    ErrorMessage = "Les mots de passe ne correspondent pas.";
                    return;
                }

                var result = await _userService.AddAsync(RegisterData);

                if (result != null)
                {
                    _navigation.NavigateTo("/connexion");
                }
                else
                {
                    ErrorMessage = "L'inscription a échoué.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur technique est survenue.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}