using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class AddressViewModel : ObservableObject
    {
        private readonly IAddressService _addressService;
        private readonly ICityService _cityService;
        private readonly IUserService _userService;
        private readonly AuthenticationStateProvider _authStateProvider;

        [ObservableProperty]
        private string deliveryCitySearch = "";

        [ObservableProperty]
        private List<CityDto> deliveryCitySuggestions = new();

        [ObservableProperty]
        private string billingCitySearch = "";

        [ObservableProperty]
        private List<CityDto> billingCitySuggestions = new();
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private string? errorMessage;

        private List<CityDto> _allCities = new();

        private int _currentUserId;
        private UserDetailDto? _currentUserDetails;

        [ObservableProperty]
        private AddressDto? deliveryAddress;

        [ObservableProperty]
        private AddressCreateDto deliveryForm = new();

        [ObservableProperty]
        private bool isEditingDelivery;

        [ObservableProperty]
        private AddressDto? billingAddress;

        [ObservableProperty]
        private AddressCreateDto billingForm = new();

        [ObservableProperty]
        private bool isEditingBilling;

        public AddressViewModel(
            IAddressService addressService,
            ICityService cityService,
            IUserService userService,
            AuthenticationStateProvider authStateProvider)
        {
            _addressService = addressService;
            _cityService = cityService;
            _userService = userService;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var userClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userClaim != null && int.TryParse(userClaim.Value, out int userId))
                {
                    _currentUserId = userId;
                    _currentUserDetails = await _userService.GetByIdAsync(userId);

                    if (_currentUserDetails != null)
                    {
                        if (_currentUserDetails.DeliveryId.HasValue && _currentUserDetails.DeliveryId.Value > 0)
                        {
                            DeliveryAddress = await _addressService.GetByIdAsync(_currentUserDetails.DeliveryId.Value);
                        }

                        if (_currentUserDetails.InvoicingId.HasValue && _currentUserDetails.InvoicingId.Value > 0)
                        {
                            BillingAddress = await _addressService.GetByIdAsync(_currentUserDetails.InvoicingId.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de charger les adresses.";
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EnsureCitiesLoadedAsync()
        {
            if (_allCities != null && _allCities.Any()) return;

            IsLoading = true;
            try
            {
                _allCities = await _cityService.GetAllAsync();
            }
            catch
            {
                ErrorMessage = "Erreur lors du chargement des villes.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void SearchDeliveryCity(string searchText)
        {
            DeliveryCitySearch = searchText;
            if (string.IsNullOrWhiteSpace(searchText) || _allCities.Count == 0)
            {
                DeliveryCitySuggestions = new List<CityDto>();
            }
            else
            {
                DeliveryCitySuggestions = _allCities
                    .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                             || c.PostCode.StartsWith(searchText))
                    .Take(10)
                    .ToList();
            }
        }

        [RelayCommand]
        public void SelectDeliveryCity(CityDto city)
        {
            DeliveryForm.CityId = city.Id;
            DeliveryCitySearch = $"{city.Name} ({city.PostCode})";
            DeliveryCitySuggestions.Clear();
        }

        [RelayCommand]
        public void SearchBillingCity(string searchText)
        {
            BillingCitySearch = searchText;
            if (string.IsNullOrWhiteSpace(searchText) || _allCities.Count == 0)
            {
                BillingCitySuggestions = new List<CityDto>();
            }
            else
            {
                BillingCitySuggestions = _allCities
                    .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                             || c.PostCode.StartsWith(searchText))
                    .Take(10)
                    .ToList();
            }
        }

        [RelayCommand]
        public void SelectBillingCity(CityDto city)
        {
            BillingForm.CityId = city.Id;
            BillingCitySearch = $"{city.Name} ({city.PostCode})";
            BillingCitySuggestions.Clear();
        }

        [RelayCommand]
        public async Task Edit(string type)
        {
            SuccessMessage = null;
            ErrorMessage = null;

            await EnsureCitiesLoadedAsync();

            if (type == "Delivery")
            {
                IsEditingDelivery = true;
                if (DeliveryAddress != null)
                {
                    DeliveryForm = new AddressCreateDto
                    {
                        CityId = DeliveryAddress.CityId,
                        StreetNumber = DeliveryAddress.StreetNumber,
                        StreetAddress = DeliveryAddress.StreetAddress
                    };
                    DeliveryCitySearch = $"{DeliveryAddress.CityName} ({DeliveryAddress.PostCode})";
                }
                else
                {
                    DeliveryForm = new AddressCreateDto();
                    DeliveryCitySearch = "";
                }
            }
            else
            {
                IsEditingBilling = true;
                if (BillingAddress != null)
                {
                    BillingForm = new AddressCreateDto
                    {
                        CityId = BillingAddress.CityId,
                        StreetNumber = BillingAddress.StreetNumber,
                        StreetAddress = BillingAddress.StreetAddress
                    };
                    BillingCitySearch = $"{BillingAddress.CityName} ({BillingAddress.PostCode})";
                }
                else
                {
                    BillingForm = new AddressCreateDto();
                    BillingCitySearch = "";
                }
            }
        }

        [RelayCommand]
        public void CancelEdit(string type)
        {
            SuccessMessage = null;
            if (type == "Delivery") IsEditingDelivery = false;
            else IsEditingBilling = false;
        }

        [RelayCommand]
        public async Task SaveAddressAsync(string type)
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            bool isDelivery = type == "Delivery";
            var form = isDelivery ? DeliveryForm : BillingForm;
            var currentAddress = isDelivery ? DeliveryAddress : BillingAddress;

            try
            {
                if (form.CityId <= 0 || string.IsNullOrWhiteSpace(form.StreetAddress) || string.IsNullOrWhiteSpace(form.StreetNumber))
                {
                    ErrorMessage = "Veuillez remplir tous les champs obligatoires.";
                    return;
                }

                if (currentAddress == null)
                {
                    var newAddress = await _addressService.AddAsync(form);
                    if (newAddress != null)
                    {
                        await LinkAddressToUserAsync(newAddress.Id, isDelivery);

                        if (isDelivery) DeliveryAddress = newAddress;
                        else BillingAddress = newAddress;

                        SuccessMessage = "Adresse créée et liée à votre compte.";
                    }
                }
                else
                {
                    var updateDto = new AddressUpdateDto
                    {
                        CityId = form.CityId,
                        StreetNumber = form.StreetNumber,
                        StreetAddress = form.StreetAddress
                    };

                    var updatedAddress = await _addressService.UpdateAsync(currentAddress.Id, updateDto);

                    if (isDelivery) DeliveryAddress = updatedAddress;
                    else BillingAddress = updatedAddress;

                    SuccessMessage = "Adresse mise à jour avec succès.";
                }

                if (isDelivery) IsEditingDelivery = false;
                else IsEditingBilling = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la sauvegarde : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteAddress(string type)
        {
            if (_currentUserDetails == null) return;

            bool isDelivery = type == "Delivery";
            int? addressIdToDelete = isDelivery ? _currentUserDetails.DeliveryId : _currentUserDetails.InvoicingId;

            if (addressIdToDelete == null) return;

            var userUpdate = new UserUpdateDto
            {
                LastName = _currentUserDetails.LastName,
                FirstName = _currentUserDetails.FirstName,
                Email = _currentUserDetails.Email,
                Username = _currentUserDetails.Username,
                Gender = _currentUserDetails.Gender,
                MultiFactorAuthentification = _currentUserDetails.MultiFactorAuthentification,
                Phone = _currentUserDetails.Phone,
                CountryId = _currentUserDetails.CountryId,
                AvatarUrl = _currentUserDetails.AvatarUrl,

                DeliveryId = isDelivery ? null : _currentUserDetails.DeliveryId,
                InvoicingId = isDelivery ? _currentUserDetails.InvoicingId : null
            };

            var userResult = await _userService.UpdateAsync(_currentUserId, userUpdate);

            if (userResult != null)
            {
                bool isUsedElsewhere = isDelivery
                    ? (_currentUserDetails.InvoicingId == addressIdToDelete)
                    : (_currentUserDetails.DeliveryId == addressIdToDelete);

                if (!isUsedElsewhere)
                {
                    await _addressService.DeleteAsync(addressIdToDelete.Value);
                }

                if (isDelivery)
                {
                    _currentUserDetails.DeliveryId = null;
                    DeliveryAddress = null;
                    DeliveryCitySearch = "";
                    DeliveryForm = new AddressCreateDto();
                }
                else
                {
                    _currentUserDetails.InvoicingId = null;
                    BillingAddress = null;
                    BillingCitySearch = "";
                    BillingForm = new AddressCreateDto();
                }

                SuccessMessage = "Adresse supprimée avec succès.";
            }
            else
            {
                ErrorMessage = "Erreur lors de la mise à jour du profil utilisateur.";
            }
        }

        private async Task LinkAddressToUserAsync(int newAddressId, bool isDelivery)
        {
            if (_currentUserDetails == null) return;

            var userUpdate = new UserUpdateDto
            {
                LastName = _currentUserDetails.LastName,
                FirstName = _currentUserDetails.FirstName,
                Email = _currentUserDetails.Email,
                Username = _currentUserDetails.Username,
                Gender = _currentUserDetails.Gender,
                MultiFactorAuthentification = _currentUserDetails.MultiFactorAuthentification,
                Phone = _currentUserDetails.Phone,
                CountryId = _currentUserDetails.CountryId,
                AvatarUrl = _currentUserDetails.AvatarUrl,
                DeliveryId = isDelivery ? newAddressId : _currentUserDetails.DeliveryId,
                InvoicingId = isDelivery ? _currentUserDetails.InvoicingId : newAddressId
            };

            var result = await _userService.UpdateAsync(_currentUserId, userUpdate);

            if (result != null)
            {
                if (isDelivery) _currentUserDetails.DeliveryId = newAddressId;
                else _currentUserDetails.InvoicingId = newAddressId;
            }
        }
    }
}