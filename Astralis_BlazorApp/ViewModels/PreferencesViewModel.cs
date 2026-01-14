using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class PreferencesViewModel : ObservableObject
    {
        private readonly IUserNotificationTypeService _preferenceService;
        private readonly AuthenticationStateProvider _authStateProvider;

        [ObservableProperty]
        private ObservableCollection<UserNotificationTypeDto> preferences = new();

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? successMessage;

        public PreferencesViewModel(
            IUserNotificationTypeService preferenceService,
            AuthenticationStateProvider authStateProvider)
        {
            _preferenceService = preferenceService;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var list = await _preferenceService.GetAllAsync();

                var sortedList = list.OrderBy(p => p.NotificationTypeName).ToList();
                Preferences = new ObservableCollection<UserNotificationTypeDto>(sortedList);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de charger vos préférences. " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task TogglePreference(UserNotificationTypeDto pref)
        {
            pref.ByMail = !pref.ByMail;

            SuccessMessage = null;
            ErrorMessage = null;

            try
            {
                var updateDto = new UserNotificationTypeUpdateDto
                {
                    NotificationTypeId = pref.NotificationTypeId,
                    ByMail = pref.ByMail
                };

                var userId = await GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _preferenceService.UpdateAsync(userId.Value, pref.NotificationTypeId, updateDto);
                    SuccessMessage = "Préférence mise à jour.";

                    _ = Task.Delay(3000).ContinueWith(_ => { SuccessMessage = null; });
                }
            }
            catch (Exception ex)
            {
                pref.ByMail = !pref.ByMail;
                ErrorMessage = "Erreur lors de la mise à jour.";
            }
        }

        private async Task<int?> GetCurrentUserId()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int id)) return id;
            }
            return null;
        }
    }
}