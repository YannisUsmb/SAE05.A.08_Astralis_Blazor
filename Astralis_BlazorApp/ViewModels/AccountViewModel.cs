using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
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

        private ValidationMessageStore? _messageStore;
        public EditContext? EditContext { get; set; }

        private int _currentUserId;
        private UserUpdateDto? _originalData;

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

                if (userClaim != null && int.TryParse(userClaim.Value, out int userId))
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

                        _originalData = new UserUpdateDto
                        {
                            Email = userDto.Email,
                            Phone = userDto.Phone
                        };

                        EditContext = new EditContext(ContactData);
                        _messageStore = new ValidationMessageStore(EditContext);
                        EditContext.OnValidationRequested += (s, e) => _messageStore.Clear();
                        EditContext.OnFieldChanged += (s, e) => _messageStore.Clear(e.FieldIdentifier);

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

        public async Task OnCountryChanged(int? newCountryId)
        {
            ContactData.CountryId = newCountryId;
            await UpdatePhonePlaceholderAsync();
            if (!string.IsNullOrWhiteSpace(ContactData.Phone)) await OnPhoneInput(ContactData.Phone);
        }

        public async Task OnPhoneInput(string input)
        {
            ContactData.Phone = input;
            if (ContactData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == ContactData.CountryId);
                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    var formatted = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.formatAsYouType", input, country.IsoCode);
                    if (ContactData.Phone != formatted) ContactData.Phone = formatted;
                }
            }
        }

        private async Task UpdatePhonePlaceholderAsync()
        {
            if (ContactData.CountryId.HasValue)
            {
                var country = Countries.FirstOrDefault(c => c.Id == ContactData.CountryId);
                if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                {
                    var example = await _jsRuntime.InvokeAsync<string>("window.phoneValidator.getExampleNumber", country.IsoCode);
                    if (!string.IsNullOrWhiteSpace(example)) PhonePlaceholder = example;
                }
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

                string? emailToCheck = (ContactData.Email != _originalData?.Email) ? ContactData.Email : null;
                string? phoneToCheck = (ContactData.Phone != _originalData?.Phone) ? cleanPhone : null;

                if (emailToCheck != null || phoneToCheck != null)
                {
                    var availability = await _userService.CheckAvailabilityAsync(emailToCheck, null, phoneToCheck);

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
                    _originalData.Email = ContactData.Email;
                    _originalData.Phone = ContactData.Phone;
                    
                    ContactData.Phone = result.Phone;

                    SuccessMessage = "Coordonnées mises à jour avec succès.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur lors de la mise à jour.";
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
            }
        }
    }
}