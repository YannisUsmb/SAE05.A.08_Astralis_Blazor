using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class MainLayoutViewModel : ObservableObject, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly IUserNotificationService _notificationService;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authStateProvider;

        private System.Timers.Timer? _pollingTimer;

        [ObservableProperty]
        private ObservableCollection<NotificationItemViewModel> notifications = new();

        [ObservableProperty]
        private int unreadCount;

        [ObservableProperty]
        private bool isNotificationDropdownOpen;

        public MainLayoutViewModel(
            IAuthService authService,
            IUserNotificationService notificationService,
            NavigationManager navigationManager,
            AuthenticationStateProvider authStateProvider)
        {
            _authService = authService;
            _notificationService = notificationService;
            _navigationManager = navigationManager;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            await LoadNotifications();

            _pollingTimer = new System.Timers.Timer(30000);
            _pollingTimer.Elapsed += async (sender, e) => await LoadNotifications();
            _pollingTimer.AutoReset = true;
            _pollingTimer.Start();
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

        private async Task LoadNotifications()
        {
            var userId = await GetCurrentUserId();
            if (userId == null) return;

            try
            {
                var dtos = await _notificationService.GetAllAsync(userId.Value);

                var orderedDtos = dtos.OrderByDescending(n => n.ReceivedAt).ToList();

                var viewModels = orderedDtos.Select(d => new NotificationItemViewModel(d));

                Notifications = new ObservableCollection<NotificationItemViewModel>(viewModels);
                UnreadCount = Notifications.Count(n => !n.Dto.IsRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur polling notifications: {ex.Message}");
            }
        }

        [RelayCommand]
        public void ToggleNotificationDropdown()
        {
            IsNotificationDropdownOpen = !IsNotificationDropdownOpen;
        }

        [RelayCommand]
        public void CloseDropdown()
        {
            IsNotificationDropdownOpen = false;
        }

        public void HandleNotificationClick(NotificationItemViewModel item)
        {
            if (!string.IsNullOrEmpty(item.Dto.Link))
            {
                IsNotificationDropdownOpen = false;
                _navigationManager.NavigateTo(item.Dto.Link);
            }
        }

        public async Task MarkAsRead(NotificationItemViewModel item)
        {
            if (item.Dto.IsRead) return;

            var userId = await GetCurrentUserId();
            if (userId == null) return;

            item.Dto.IsRead = true;
            UnreadCount = Notifications.Count(n => !n.Dto.IsRead);

            int userNotificationId = item.Dto.Id;

            var updateDto = new UserNotificationUpdateDto
            {
                UserId = userId.Value,
                NotificationId = item.Dto.NotificationId,
                IsRead = true
            };

            await _notificationService.UpdateAsync(userNotificationId, updateDto);
        }

        public async Task DeleteNotification(NotificationItemViewModel item)
        {
            var userId = await GetCurrentUserId();
            if (userId == null) return;

            Notifications.Remove(item);
            UnreadCount = Notifications.Count(n => !n.Dto.IsRead);

            await _notificationService.DeleteAsync(item.Dto.Id);
        }

        public async Task LogoutAsync()
        {
            _pollingTimer?.Stop();
            await _authService.Logout();
            _navigationManager.NavigateTo("connexion", forceLoad: true);
        }

        public void Dispose()
        {
            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
        }
    }

    public partial class NotificationItemViewModel : ObservableObject
    {
        public UserNotificationDto Dto { get; set; }

        [ObservableProperty]
        private bool isExpanded;

        public bool CanExpand => !string.IsNullOrEmpty(Dto.Description) && Dto.Description.Length > 60;

        public NotificationItemViewModel(UserNotificationDto dto)
        {
            Dto = dto;
            IsExpanded = false;
        }
    }
}