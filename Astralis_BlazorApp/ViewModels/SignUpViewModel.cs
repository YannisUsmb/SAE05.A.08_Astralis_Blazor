using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICountryService _countryService; // Injection du nouveau service
        private readonly NavigationManager _navigation;

        [ObservableProperty]
        private UserCreateDto registerData = new()
        {
            Gender = GenderType.Male,
            MultiFactorAuthentification = false
        };

        // Liste des pays pour le menu déroulant
        [ObservableProperty]
        private ObservableCollection<CountryDto> countries = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public SignUpViewModel(IUserService userService, ICountryService countryService, NavigationManager navigation)
        {
            _userService = userService;
            _countryService = countryService;
            _navigation = navigation;
        }

        public async Task LoadCountriesAsync()
        {
            try
            {
                var list = await _countryService.GetAllAsync();
                // On trie par nom alphabétique pour faciliter la recherche utilisateur
                var sortedList = list.OrderBy(c => c.Name).ToList();
                Countries = new ObservableCollection<CountryDto>(sortedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement pays: {ex.Message}");
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