using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICountryService _countryService;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _jsRuntime;

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

        [ObservableProperty]
        private string phonePlaceholder = "06 12 34 56 78";

        private ValidationMessageStore? _messageStore;
        public EditContext? EditContext { get; set; }

        public SignUpViewModel(
            IUserService userService,
            ICountryService countryService,
            NavigationManager navigation,
            IJSRuntime jsRuntime)
        {
            _userService = userService;
            _countryService = countryService;
            _navigation = navigation;
            _jsRuntime = jsRuntime;
        }

        public void InitializeContext()
        {
            EditContext = new EditContext(RegisterData);
            EditContext.OnValidationRequested += (s, e) => _messageStore?.Clear();
            EditContext.OnFieldChanged += (s, e) => _messageStore?.Clear(e.FieldIdentifier);
            _messageStore = new ValidationMessageStore(EditContext);
        }

        public async Task LoadCountriesAsync()
        {
            try
            {
                var list = await _countryService.GetAllAsync();
                Countries = new ObservableCollection<CountryDto>(list.OrderBy(c => c.Name));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur pays: {ex.Message}");
            }
        }

        public async Task OnCountryChanged(int? countryId)
        {
            RegisterData.CountryId = countryId;
            await UpdatePhonePlaceholderAsync();
            if (!string.IsNullOrWhiteSpace(RegisterData.Phone))
            {
                await OnPhoneInput(RegisterData.Phone);
            }
        }

        public async Task OnPhoneInput(string input)
        {
            RegisterData.Phone = input;
            if (RegisterData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    var formatted = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.formatAsYouType", input, country.IsoCode);
                    if (RegisterData.Phone != formatted) RegisterData.Phone = formatted;
                }
            }
        }

        private async Task UpdatePhonePlaceholderAsync()
        {
            if (RegisterData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    var example = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.getExampleNumber", country.IsoCode);
                    if (!string.IsNullOrWhiteSpace(example)) PhonePlaceholder = example;
                }
            }
            else
            {
                PhonePlaceholder = "06 12 34 56 78";
            }
        }

        public async Task ValidatePhoneOnBlurAsync()
        {
            if (string.IsNullOrWhiteSpace(RegisterData.Phone)) return;

            if (!RegisterData.CountryId.HasValue)
            {
                EditContext?.NotifyFieldChanged(EditContext.Field(nameof(RegisterData.CountryId)));
                return;
            }

            var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
            if (country != null && !string.IsNullOrEmpty(country.IsoCode))
            {
                bool isValid = await _jsRuntime.InvokeAsync<bool>("window.phoneValidator.isValid", RegisterData.Phone, country.IsoCode);
                if (!isValid && EditContext != null)
                {
                    var field = EditContext.Field(nameof(RegisterData.Phone));
                    _messageStore?.Add(field, $"Numéro invalide pour ce pays ({country.Name}).");
                    EditContext.NotifyValidationStateChanged();
                }
            }
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = null;
            _messageStore?.Clear();

            if (EditContext != null && !EditContext.Validate())
            {
                IsLoading = false;
                return;
            }

            try
            {
                if (RegisterData.Password != RegisterData.ConfirmPassword)
                {
                    if (EditContext != null)
                    {
                        _messageStore?.Add(EditContext.Field(nameof(RegisterData.ConfirmPassword)), "Les mots de passe ne correspondent pas.");
                        EditContext.NotifyValidationStateChanged();
                    }
                    IsLoading = false;
                    return;
                }

                if (!string.IsNullOrEmpty(RegisterData.Phone)) RegisterData.Phone = RegisterData.Phone.Replace(" ", "");

                var availability = await _userService.CheckAvailabilityAsync(
                    RegisterData.Email,
                    RegisterData.Username,
                    RegisterData.Phone
                );

                if (availability != null && availability.IsTaken)
                {
                    var fieldName = availability.Field?.ToLower() switch
                    {
                        "email" => nameof(RegisterData.Email),
                        "username" => nameof(RegisterData.Username),
                        "phone" => nameof(RegisterData.Phone),
                        _ => string.Empty
                    };

                    if (!string.IsNullOrEmpty(fieldName) && EditContext != null)
                    {
                        _messageStore?.Add(EditContext.Field(fieldName), availability.Message ?? "Ce champ est déjà utilisé.");
                        EditContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        ErrorMessage = availability.Message ?? "Ces informations sont déjà utilisées.";
                    }

                    IsLoading = false;
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