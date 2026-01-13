using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Authorization;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class AddressViewModel : ObservableObject
    {
        private readonly IAddressService _addressService;
        private readonly ICityService _cityService;
        private readonly IUserService _userService;
        private readonly AuthenticationStateProvider _authStateProvider;

        private CancellationTokenSource? _searchCts;

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
                            DeliveryAddress = await _addressService.GetByIdAsync(_currentUserDetails.DeliveryId.Value);

                        if (_currentUserDetails.InvoicingId.HasValue && _currentUserDetails.InvoicingId.Value > 0)
                            BillingAddress = await _addressService.GetByIdAsync(_currentUserDetails.InvoicingId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur chargement profil.";
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchDeliveryCity(string searchText)
        {
            DeliveryCitySearch = searchText;

            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 3)
            {
                DeliveryCitySuggestions.Clear();
                return;
            }

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(400, token);

                var results = await _cityService.SearchAsync(searchText);

                if (!token.IsCancellationRequested)
                {
                    DeliveryCitySuggestions = results;
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        [RelayCommand]
        public void SelectDeliveryCity(CityDto city)
        {
            _searchCts?.Cancel();

            DeliveryForm.CityId = city.Id;
            DeliveryCitySearch = $"{city.Name} ({city.PostCode})";
            DeliveryCitySuggestions.Clear();
        }

        [RelayCommand]
        public async Task SearchBillingCity(string searchText)
        {
            BillingCitySearch = searchText;

            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 3)
            {
                BillingCitySuggestions.Clear();
                return;
            }

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(400, token);
                var results = await _cityService.SearchAsync(searchText);

                if (!token.IsCancellationRequested)
                {
                    BillingCitySuggestions = results;
                }
            }
            catch (TaskCanceledException) { }
        }

        [RelayCommand]
        public void SelectBillingCity(CityDto city)
        {
            _searchCts?.Cancel();

            BillingForm.CityId = city.Id;
            BillingCitySearch = $"{city.Name} ({city.PostCode})";
            BillingCitySuggestions.Clear();
        }


        [RelayCommand]
        public void Edit(string type)
        {
            SuccessMessage = null;
            ErrorMessage = null;

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
        public async Task SaveAddress(string type)
        {
            if (_currentUserDetails == null) return;

            bool isDelivery = type == "Delivery";
            var form = isDelivery ? DeliveryForm : BillingForm;

            if (string.IsNullOrWhiteSpace(form.StreetAddress))
            {
                ErrorMessage = "L'adresse est incomplète (la rue est requise).";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var createdAddress = await _addressService.AddAsync(form);

                if (createdAddress == null) throw new Exception("Erreur : L'API n'a pas renvoyé l'adresse créée.");

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
                    DeliveryId = isDelivery ? createdAddress.Id : _currentUserDetails.DeliveryId,
                    InvoicingId = isDelivery ? _currentUserDetails.InvoicingId : createdAddress.Id
                };

                try
                {
                    await _userService.UpdateAsync(_currentUserId, userUpdate);
                }
                catch (Exception) {  }

                if (isDelivery)
                {
                    DeliveryAddress = createdAddress;
                    _currentUserDetails.DeliveryId = createdAddress.Id;
                    IsEditingDelivery = false;
                }
                else
                {
                    BillingAddress = createdAddress;
                    _currentUserDetails.InvoicingId = createdAddress.Id;
                    IsEditingBilling = false;
                }

                SuccessMessage = "Adresse enregistrée avec succès.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
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

            bool isUsedElsewhere = isDelivery
                ? (_currentUserDetails.InvoicingId == addressIdToDelete)
                : (_currentUserDetails.DeliveryId == addressIdToDelete);

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

            try
            {
                IsLoading = true;

                await _userService.UpdateAsync(_currentUserId, userUpdate);

                if (!isUsedElsewhere)
                {
                    await _addressService.DeleteAsync(addressIdToDelete.Value);
                }
                ForceDeleteUI(isDelivery);

                SuccessMessage = "Adresse supprimée avec succès.";
            }
            catch (Exception ex)
            {
                ForceDeleteUI(isDelivery);
                SuccessMessage = "Adresse supprimée.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ForceDeleteUI(bool isDelivery)
        {
            if (isDelivery)
            {
                if (_currentUserDetails != null) _currentUserDetails.DeliveryId = null;
                DeliveryAddress = null;
                DeliveryCitySearch = "";
                DeliveryForm = new AddressCreateDto();
            }
            else
            {
                if (_currentUserDetails != null) _currentUserDetails.InvoicingId = null;
                BillingAddress = null;
                BillingCitySearch = "";
                BillingForm = new AddressCreateDto();
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