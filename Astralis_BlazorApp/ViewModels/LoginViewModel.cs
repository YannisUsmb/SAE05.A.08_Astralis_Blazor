using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ICountryService _countryService;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        private UserLoginDto loginData = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private LoginMode selectedLoginMode = LoginMode.Standard;

        [ObservableProperty]
        private ObservableCollection<CountryDto> countries = new();

        public LoginViewModel(
            IAuthService authService,
            ICountryService countryService,
            NavigationManager navigation,
            IJSRuntime jsRuntime)
        {
            _authService = authService;
            _countryService = countryService;
            _navigation = navigation;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadCountriesAsync()
        {
            var list = await _countryService.GetAllAsync();
            Countries = new ObservableCollection<CountryDto>(list.OrderBy(c => c.Name));
        }

        public async Task FormatPhoneNumber()
        {
            if (!string.IsNullOrWhiteSpace(LoginData.Phone) && LoginData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == LoginData.CountryId);

                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    string formatted = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.formatAsYouType", LoginData.Phone, country.IsoCode);
                    LoginData.Phone = formatted;
                }
            }
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                if (SelectedLoginMode == LoginMode.Standard)
                {
                    LoginData.Phone = null;
                    LoginData.CountryId = null;

                    if (string.IsNullOrWhiteSpace(LoginData.Identifier))
                    {
                        ErrorMessage = "Veuillez saisir un identifiant.";
                        IsLoading = false;
                        return;
                    }
                }
                else
                {
                    LoginData.Identifier = null;

                    if (string.IsNullOrWhiteSpace(LoginData.Phone) || !LoginData.CountryId.HasValue)
                    {
                        ErrorMessage = "Veuillez saisir un numéro et choisir un pays.";
                        IsLoading = false;
                        return;
                    }

                    LoginData.Phone = LoginData.Phone.Replace(" ", "");
                }

                if (string.IsNullOrWhiteSpace(LoginData.Password))
                {
                    ErrorMessage = "Le mot de passe est requis.";
                    IsLoading = false;
                    return;
                }

                var result = await _authService.Login(LoginData);

                if (result != null)
                {
                    _navigation.NavigateTo("/");
                }
                else
                {
                    ErrorMessage = "Identifiants incorrects.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ErrorMessage = "Une erreur technique est survenue.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoginWithGoogleAsync(string idToken)
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var googleDto = new GoogleLoginDto { IdToken = idToken };
                var result = await _authService.GoogleLogin(googleDto);

                if (result != null)
                {
                    _navigation.NavigateTo("/");
                }
                else
                {
                    ErrorMessage = "Impossible de se connecter avec Google.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Google: {ex.Message}");
                ErrorMessage = "Une erreur technique est survenue.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}