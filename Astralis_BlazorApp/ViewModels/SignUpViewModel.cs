using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Implementations;
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
        private readonly IAuthService _authService;
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

        [ObservableProperty]
        private string phonePlaceholder = "Choisir un pays...";

        [ObservableProperty]
        private bool isTermsAccepted;

        private ValidationMessageStore? _messageStore;
        public EditContext? EditContext { get; set; }

        public SignUpViewModel(
            IUserService userService,
            ICountryService countryService,
            IAuthService authService,
            NavigationManager navigation)
        {
            _userService = userService;
            _countryService = countryService;
            _authService = authService;
            _navigation = navigation;
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

        public void OnCountryChanged(int? countryId)
        {
            RegisterData.CountryId = countryId;

            if (!countryId.HasValue)
            {
                RegisterData.Phone = string.Empty;
                PhonePlaceholder = "Choisir un pays d'abord...";
            }
            else if (EditContext != null)
            {
                var countryField = EditContext.Field(nameof(RegisterData.CountryId));
                var phoneField = EditContext.Field(nameof(RegisterData.Phone));
                _messageStore?.Clear(countryField);
                _messageStore?.Clear(phoneField);
                EditContext.NotifyValidationStateChanged();
            }

            UpdatePhonePlaceholder();
        }

        private void UpdatePhonePlaceholder()
        {
            if (RegisterData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
                PhonePlaceholder = country?.PhoneExample ?? "Numéro de téléphone";
            }
            else
            {
                PhonePlaceholder = "Choisir un pays d'abord...";
            }
        }

        public void OnPhoneInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input) && EditContext != null)
            {
                _messageStore?.Clear(EditContext.Field(nameof(RegisterData.Phone)));
                EditContext.NotifyValidationStateChanged();
            }

            var sanitized = new string(input.Where(c => char.IsDigit(c) || c == ' ').ToArray());

            RegisterData.Phone = sanitized;
        }

        public async Task ValidateAvailabilityOnBlur(string fieldName)
        {
            string? valueToCheck = fieldName switch
            {
                nameof(RegisterData.Email) => RegisterData.Email,
                nameof(RegisterData.Username) => RegisterData.Username,
                nameof(RegisterData.Phone) => RegisterData.Phone,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(valueToCheck)) return;

            string? phoneToSend = fieldName == nameof(RegisterData.Phone)
                ? RegisterData.Phone?.Replace(" ", "")
                : null;

            string? countryIdToSend = fieldName == nameof(RegisterData.Phone) && RegisterData.CountryId.HasValue
                ? RegisterData.CountryId.Value.ToString()
                : null;

            var result = await _userService.CheckAvailabilityAsync(
                fieldName == nameof(RegisterData.Email) ? RegisterData.Email : null,
                fieldName == nameof(RegisterData.Username) ? RegisterData.Username : null,
                phoneToSend, countryIdToSend
            );

            if (EditContext != null)
            {
                var fieldIdentifier = EditContext.Field(fieldName);
                _messageStore?.Clear(fieldIdentifier);

                if (result != null && result.IsTaken)
                {
                    _messageStore?.Add(fieldIdentifier, result.Message ?? "Déjà pris.");
                }

                if (fieldName == nameof(RegisterData.Phone))
                {
                    ValidatePhoneFormat();
                }

                EditContext.NotifyValidationStateChanged();
            }
        }

        private void ValidatePhoneFormat()
        {
            if (!RegisterData.CountryId.HasValue || string.IsNullOrWhiteSpace(RegisterData.Phone)) return;

            var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
            if (country != null && !string.IsNullOrEmpty(country.PhoneRegex))
            {
                try
                {
                    string phoneClean = new string(RegisterData.Phone.Where(char.IsDigit).ToArray());

                    bool isValid = System.Text.RegularExpressions.Regex.IsMatch(phoneClean, country.PhoneRegex);

                    if (!isValid && phoneClean.StartsWith("0") && phoneClean.Length > 1)
                    {
                        string phoneWithoutZero = phoneClean.Substring(1);
                        if (System.Text.RegularExpressions.Regex.IsMatch(phoneWithoutZero, country.PhoneRegex))
                        {
                            isValid = true;
                        }
                    }

                    if (!isValid)
                    {
                        _messageStore?.Add(EditContext!.Field(nameof(RegisterData.Phone)), "Format invalide pour ce pays.");
                    }
                }
                catch
                { }
            }
        }

        public async Task RegisterWithGoogleAsync(string idToken)
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
                    ErrorMessage = "L'inscription avec Google a échoué.";
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

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = null;
            _messageStore?.Clear();

            if (!IsTermsAccepted)
            {
                ErrorMessage = "Vous devez accepter les Conditions Générales d'Utilisation et la Politique de Confidentialité pour créer un compte.";
                IsLoading = false;
                return;
            }

            if (EditContext != null && !EditContext.Validate())
            {
                IsLoading = false;
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(RegisterData.Phone))
                {
                    string cleanPhone = new string(RegisterData.Phone.Where(char.IsDigit).ToArray());

                    var country = Countries.FirstOrDefault(c => c.Id == RegisterData.CountryId);
                    if (country != null && !string.IsNullOrEmpty(country.PhoneRegex))
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(cleanPhone, country.PhoneRegex)
                            && cleanPhone.StartsWith("0"))
                        {
                            var noZero = cleanPhone.Substring(1);
                            if (System.Text.RegularExpressions.Regex.IsMatch(noZero, country.PhoneRegex))
                            {
                                cleanPhone = noZero;
                            }
                        }
                    }
                    RegisterData.Phone = cleanPhone;
                }

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
                    RegisterData.Phone,
                    RegisterData.CountryId.HasValue ? RegisterData.CountryId.Value.ToString() : null
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
                    _navigation.NavigateTo("/inscription/confirmation");
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