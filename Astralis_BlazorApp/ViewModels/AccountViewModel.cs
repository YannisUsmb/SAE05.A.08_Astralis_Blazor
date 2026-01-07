using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICountryService _countryService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsContactDirty))]
        private UserUpdateDto contactData = new();

        [ObservableProperty]
        private ObservableCollection<CountryDto> countries = new();

        [ObservableProperty]
        private string phonePlaceholder = "Numéro de téléphone";

        [ObservableProperty]
        private ChangePasswordDto passwordModel = new();

        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private string? successMessage;
        [ObservableProperty]
        private string? errorMessage;

        public EditContext? EditContext { get; set; }
        private ValidationMessageStore? _messageStore;

        private int _currentUserId;
        private UserUpdateDto _originalContactData = new();

        public bool IsContactDirty =>
            ContactData.Email != _originalContactData.Email ||
            ContactData.Phone != _originalContactData.Phone ||
            ContactData.CountryId != _originalContactData.CountryId;

        public AccountViewModel(IUserService userService, ICountryService countryService, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _userService = userService;
            _countryService = countryService;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadUserDataAsync()
        {
            IsLoading = true;
            try
            {
                var countriesList = await _countryService.GetAllAsync();
                Countries = new ObservableCollection<CountryDto>(countriesList.OrderBy(c => c.Name));

                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var userClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userClaim != null && int.TryParse(userClaim.Value, out int userId) && userId > 0)
                {
                    _currentUserId = userId;
                    var userDto = await _userService.GetByIdAsync(userId);

                    if (userDto != null)
                    {
                        ContactData = new UserUpdateDto
                        {
                            Email = userDto.Email,
                            Phone = userDto.Phone,
                            CountryId = userDto.CountryId,
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Username = userDto.Username,
                            AvatarUrl = userDto.AvatarUrl,
                            Gender = userDto.Gender,
                            MultiFactorAuthentification = userDto.MultiFactorAuthentification
                        };

                        _originalContactData = new UserUpdateDto
                        {
                            Email = userDto.Email,
                            Phone = userDto.Phone,
                            CountryId = userDto.CountryId
                        };

                        InitializeEditContext();
                        await UpdatePhonePlaceholderAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de charger les données.";
            }
            finally { IsLoading = false; }
        }

        private void InitializeEditContext()
        {
            EditContext = new EditContext(ContactData);
            _messageStore = new ValidationMessageStore(EditContext);

            EditContext.OnFieldChanged += (s, e) => OnPropertyChanged(nameof(IsContactDirty));
            EditContext.OnValidationRequested += (s, e) => _messageStore.Clear();
        }

        public async Task OnCountryChanged(int? newCountryId)
        {
            ContactData.CountryId = newCountryId;
            OnPropertyChanged(nameof(ContactData));
            OnPropertyChanged(nameof(IsContactDirty));

            await UpdatePhonePlaceholderAsync();

            if (!string.IsNullOrWhiteSpace(ContactData.Phone))
                await OnPhoneInput(ContactData.Phone);
        }

        public async Task OnPhoneInput(string input)
        {
            ContactData.Phone = input;
            if (ContactData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == ContactData.CountryId);
                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    try
                    {
                        var formatted = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.formatAsYouType", input, country.IsoCode);
                        if (ContactData.Phone != formatted) ContactData.Phone = formatted;
                    }
                    catch { }
                }
            }
        }

        private async Task UpdatePhonePlaceholderAsync()
        {
            if (ContactData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == ContactData.CountryId);
                if (country != null)
                {
                    PhonePlaceholder = country.PhoneExample ?? "06 12 34 56 78";
                }
            }
            else
            {
                PhonePlaceholder = "Choisir un pays...";
            }
        }

        [RelayCommand]
        public async Task SaveContactInfoAsync()
        {
            if (EditContext == null || !EditContext.Validate()) return;

            IsLoading = true;
            SuccessMessage = null;
            ErrorMessage = null;
            _messageStore?.Clear();

            try
            {
                string? cleanPhone = !string.IsNullOrEmpty(ContactData.Phone) ? ContactData.Phone.Replace(" ", "") : null;
                string? emailToCheck = (ContactData.Email != _originalContactData.Email) ? ContactData.Email : null;
                string? phoneToCheck = (ContactData.Phone != _originalContactData.Phone) ? cleanPhone : null;
                string? countryIdToCheck = ContactData.CountryId.HasValue ? ContactData.CountryId.ToString() : null;

                if (emailToCheck != null || phoneToCheck != null)
                {
                    var availability = await _userService.CheckAvailabilityAsync(emailToCheck, null, phoneToCheck, countryIdToCheck);

                    if (availability != null && availability.IsTaken)
                    {
                        var fieldName = availability.Field?.ToLower() switch
                        {
                            "email" => nameof(ContactData.Email),
                            "phone" => nameof(ContactData.Phone),
                            _ => string.Empty
                        };

                        if (!string.IsNullOrEmpty(fieldName))
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
                }

                ContactData.Phone = cleanPhone;

                var result = await _userService.UpdateAsync(_currentUserId, ContactData);

                if (result != null)
                {
                    _originalContactData.Email = ContactData.Email;
                    _originalContactData.Phone = ContactData.Phone;
                    _originalContactData.CountryId = ContactData.CountryId;

                    await OnPhoneInput(ContactData.Phone ?? "");

                    SuccessMessage = "Coordonnées mises à jour avec succès.";
                    OnPropertyChanged(nameof(IsContactDirty));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur lors de la mise à jour.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ChangePasswordAsync()
        {
            SuccessMessage = null;
            ErrorMessage = null;

            try
            {
                await _userService.ChangePasswordAsync(_currentUserId, PasswordModel);
                SuccessMessage = "Mot de passe modifié avec succès.";
                PasswordModel = new ChangePasswordDto();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de modifier le mot de passe.";
            }
        }
    }
}