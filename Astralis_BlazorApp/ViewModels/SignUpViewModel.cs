using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Constants;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly NavigationManager _navigation;

        [ObservableProperty]
        private UserCreateDto registerData = new()
        {
            Gender = GenderType.Unknown,
            MultiFactorAuthentification = false
        };

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public SignUpViewModel(IUserService userService, NavigationManager navigation)
        {
            _userService = userService;
            _navigation = navigation;
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (isLoading) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                // Validation simple (le reste est géré par le DataAnnotationsValidator dans la vue)
                if (RegisterData.Password != RegisterData.ConfirmPassword)
                {
                    ErrorMessage = "Les mots de passe ne correspondent pas.";
                    return;
                }

                var result = await _userService.AddAsync(RegisterData);

                if (result != null)
                {
                    // Redirection vers la connexion après succès
                    // Ou connexion directe selon ta logique backend
                    _navigation.NavigateTo(AppRoutes.Connexion);
                }
                else
                {
                    ErrorMessage = "L'inscription a échoué. Veuillez réessayer.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Inscription : {ex.Message}");
                ErrorMessage = "Une erreur est survenue lors de l'inscription.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}