using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigation;

        [ObservableProperty]
        private UserLoginDto loginData = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public LoginViewModel(IAuthService authService, NavigationManager navigation)
        {
            _authService = authService;
            _navigation = navigation;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (isLoading)
                return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                if (string.IsNullOrWhiteSpace(LoginData.Identifier) || string.IsNullOrWhiteSpace(LoginData.Password))
                {
                    ErrorMessage = "Veuillez remplir tous les champs.";
                    return;
                }

                var result = await _authService.Login(LoginData);

                if (result != null)
                {
                    _navigation.NavigateTo("/");
                }
                else
                {
                    ErrorMessage = "Identifiant ou mot de passe incorrect.";
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
    }
}