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
        private readonly IUploadService _uploadService;
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

        public bool IsUploading { get; private set; } = false;

        private ValidationMessageStore? _messageStore;
        public EditContext? EditContext { get; set; }

        public SignUpViewModel(
            IUserService userService,
            ICountryService countryService,
            IUploadService uploadService,
            NavigationManager navigation,
            IJSRuntime jsRuntime)
        {
            _userService = userService;
            _countryService = countryService;
            _navigation = navigation;
            _uploadService = uploadService;
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
                var sortedList = list.OrderBy(c => c.Name).ToList();
                Countries = new ObservableCollection<CountryDto>(sortedList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement pays: {ex.Message}");
            }
        }


        public async Task OnCountryChanged(int? countryId)
        {
            RegisterData.CountryId = countryId;

            if (EditContext != null)
            {
                var field = EditContext.Field(nameof(RegisterData.Phone));
                _messageStore?.Clear(field);
                EditContext.NotifyValidationStateChanged();
            }

            await UpdatePhonePlaceholderAsync();

            if (!string.IsNullOrWhiteSpace(RegisterData.Phone))
            {
                await OnPhoneInput(RegisterData.Phone);
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
                    if (!string.IsNullOrWhiteSpace(example))
                    {
                        PhonePlaceholder = example;
                    }
                }
            }
            else
            {
                PhonePlaceholder = "06 12 34 56 78";
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

                    if (RegisterData.Phone != formatted)
                    {
                        RegisterData.Phone = formatted;
                    }
                }
            }
        }

        public async Task ValidatePhoneOnBlurAsync()
        {
            if (string.IsNullOrWhiteSpace(RegisterData.Phone))
            {
                return;
            }

            if (!RegisterData.CountryId.HasValue)
            {
                EditContext?.NotifyFieldChanged(EditContext.Field(nameof(RegisterData.CountryId)));
                return;
            }

            var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
            if (country != null && !string.IsNullOrEmpty(country.IsoCode))
            {
                // Appel JS pour vérifier la validité
                bool isValid = await _jsRuntime.InvokeAsync<bool>("window.phoneValidator.isValid", RegisterData.Phone, country.IsoCode);

                if (!isValid)
                {
                    if (EditContext != null)
                    {
                        var field = EditContext.Field(nameof(RegisterData.Phone));
                        _messageStore?.Clear(field); // On nettoie avant d'ajouter
                        _messageStore?.Add(field, $"Numéro invalide pour ce pays ({country.Name}).");
                        EditContext.NotifyValidationStateChanged();
                    }
                }
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
                    ErrorMessage = "Les mots de passe ne correspondent pas.";
                    IsLoading = false;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(RegisterData.Phone) && RegisterData.CountryId.HasValue)
                {
                    await ValidatePhoneOnBlurAsync();
                    if (EditContext!.GetValidationMessages(EditContext.Field(nameof(RegisterData.Phone))).Any())
                    {
                        IsLoading = false;
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(RegisterData.Phone))
                {
                    RegisterData.Phone = RegisterData.Phone.Replace(" ", "");
                }

                var availability = await _userService.CheckAvailabilityAsync(
                    RegisterData.Email,
                    RegisterData.Username,
                    RegisterData.Phone
                );

                if (availability != null && availability.IsTaken)
                {
                    var fieldName = availability.Field switch
                    {
                        "email" => nameof(RegisterData.Email),
                        "username" => nameof(RegisterData.Username),
                        "phone" => nameof(RegisterData.Phone),
                        _ => string.Empty
                    };

                    if (!string.IsNullOrEmpty(fieldName) && EditContext != null)
                    {
                        _messageStore?.Add(EditContext.Field(fieldName), availability.Message ?? "Déjà utilisé.");
                        EditContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        ErrorMessage = availability.Message;
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
                    ErrorMessage = "L'inscription a échoué. Veuillez vérifier vos informations.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ErrorMessage = "Une erreur technique est survenue lors de l'inscription.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}