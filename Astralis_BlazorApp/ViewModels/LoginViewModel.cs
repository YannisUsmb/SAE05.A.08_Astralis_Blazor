using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ICountryService _countryService;
        private readonly NavigationManager _navigation;

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

        [ObservableProperty]
        private string phonePlaceholder = "Choisir un pays...";

        private ValidationMessageStore? _messageStore;

        private string? _returnUrl;

        public EditContext? EditContext { get; set; }

        public LoginViewModel(
            IAuthService authService,
            ICountryService countryService,
            NavigationManager navigation)
        {
            _authService = authService;
            _countryService = countryService;
            _navigation = navigation;
        }

        public void InitializeContext()
        {
            EditContext = new EditContext(LoginData);
            EditContext.OnValidationRequested += (s, e) => _messageStore?.Clear();
            EditContext.OnFieldChanged += (s, e) => _messageStore?.Clear(e.FieldIdentifier);
            _messageStore = new ValidationMessageStore(EditContext);

            var uri = _navigation.ToAbsoluteUri(_navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var returnUrlFromQuery))
            {
                _returnUrl = returnUrlFromQuery;
            }
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

        public void ToggleLoginMode(LoginMode mode)
        {
            SelectedLoginMode = mode;
            ErrorMessage = null;

            if (_messageStore != null)
            {
                _messageStore.Clear();
            }

            if (mode == LoginMode.Standard)
            {
                LoginData.Phone = null;
                LoginData.CountryId = null;
                PhonePlaceholder = "Choisir un pays...";
            }
            else
            {
                LoginData.Identifier = null;
            }

            EditContext?.NotifyValidationStateChanged();
        }

        public void OnCountryChanged(int? countryId)
        {
            LoginData.CountryId = countryId;

            if (!countryId.HasValue)
            {
                LoginData.Phone = string.Empty;
                PhonePlaceholder = "Choisir un pays d'abord...";
            }
            else if (EditContext != null)
            {
                _messageStore?.Clear(EditContext.Field(nameof(LoginData.CountryId)));
                _messageStore?.Clear(EditContext.Field(nameof(LoginData.Phone)));
                EditContext.NotifyValidationStateChanged();
            }

            UpdatePhonePlaceholder();
        }

        private void UpdatePhonePlaceholder()
        {
            if (LoginData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == LoginData.CountryId);
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
                _messageStore?.Clear(EditContext.Field(nameof(LoginData.Phone)));
            }

            var sanitized = new string(input.Where(c => char.IsDigit(c) || c == ' ').ToArray());
            LoginData.Phone = sanitized;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = null;

            _messageStore?.Clear();

            if (SelectedLoginMode == LoginMode.Standard)
            {
                LoginData.CountryId = null;
                LoginData.Phone = null;
            }
            else
            {
                LoginData.Identifier = null;
                if (!string.IsNullOrEmpty(LoginData.Phone))
                {
                    string cleanPhone = new string(LoginData.Phone.Where(char.IsDigit).ToArray());
                    var country = Countries.FirstOrDefault(c => c.Id == LoginData.CountryId);
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
                    LoginData.Phone = cleanPhone;
                }
            }

            if (EditContext != null && !EditContext.Validate())
            {
                IsLoading = false;
                return;
            }

            try
            {
                var result = await _authService.Login(LoginData);

                if (result != null)
                {
                    _navigation.NavigateTo(!string.IsNullOrEmpty(_returnUrl) ? _returnUrl : "/");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ErrorMessage = ex.Message;
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
                    _navigation.NavigateTo(!string.IsNullOrEmpty(_returnUrl) ? _returnUrl : "/");
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