using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventDetailsViewModel : ObservableObject
    {
        private readonly IEventService _eventService;
        private readonly IEventInterestService _interestService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty] private EventDto? selectedEvent;
        [ObservableProperty] private bool isLoading = true;

        [ObservableProperty] private bool canManage;
        [ObservableProperty] private bool isDeleteModalOpen;

        public EventDetailsViewModel(
            IEventService eventService,
            IEventInterestService interestService,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation,
            IJSRuntime jsRuntime)
        {
            _eventService = eventService;
            _interestService = interestService;
            _authStateProvider = authStateProvider;
            _navigation = navigation;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(int eventId)
        {
            IsLoading = true;
            try
            {
                SelectedEvent = await _eventService.GetByIdAsync(eventId);

                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                if (user.Identity?.IsAuthenticated == true && SelectedEvent != null)
                {
                    bool isAdmin = user.IsInRole("Admin");
                    var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
                    {
                        CanManage = isAdmin || userId == SelectedEvent.UserId;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                _navigation.NavigateTo("/evenements");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void NavigateToEdit()
        {
            if (SelectedEvent != null)
                _navigation.NavigateTo($"/admin/evenements/edition/{SelectedEvent.Id}");
        }

        [RelayCommand]
        public void OpenDeleteModal() => IsDeleteModalOpen = true;

        [RelayCommand]
        public void CloseDeleteModal() => IsDeleteModalOpen = false;

        [RelayCommand]
        public async Task ConfirmDeleteAsync()
        {
            if (SelectedEvent == null) return;
            try
            {
                await _eventService.DeleteAsync(SelectedEvent.Id);
                _navigation.NavigateTo("/evenements");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression: {ex.Message}");
                CloseDeleteModal();
            }
        }

        public async Task AddToCalendar()
        {
            if (SelectedEvent == null) return;
            string icsContent = GenerateIcsContent(SelectedEvent);
            string fileName = $"astralis-event-{SelectedEvent.Id}.ics";
            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "text/calendar", icsContent);
        }

        public async Task ToggleInterest()
        {
            if (SelectedEvent == null) return;
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true) return;
            var userIdStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return;

            bool wasInterested = SelectedEvent.IsInterested;
            SelectedEvent.IsInterested = !SelectedEvent.IsInterested;
            OnPropertyChanged(nameof(SelectedEvent));

            try
            {
                if (wasInterested) await _interestService.DeleteAsync(SelectedEvent.Id, userId);
                else await _interestService.AddAsync(new EventInterestDto { EventId = SelectedEvent.Id, UserId = userId });
            }
            catch
            {
                SelectedEvent.IsInterested = wasInterested;
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }

        private string GenerateIcsContent(EventDto evt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//Astralis//NONSGML Event//FR");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:astralis-{evt.Id}-{DateTime.Now.Ticks}@astralis.space");
            sb.AppendLine($"DTSTAMP:{FormatDateToIcs(DateTime.UtcNow)}");
            sb.AppendLine($"DTSTART:{FormatDateToIcs(evt.StartDate.ToUniversalTime())}");
            var endDate = evt.EndDate ?? evt.StartDate.AddHours(1);
            sb.AppendLine($"DTEND:{FormatDateToIcs(endDate.ToUniversalTime())}");
            sb.AppendLine($"SUMMARY:{EscapeIcsText(evt.Title)}");
            string plainDescription = StripHtml(evt.Description);
            sb.AppendLine($"DESCRIPTION:{EscapeIcsText(plainDescription)}");
            if (!string.IsNullOrEmpty(evt.Location)) sb.AppendLine($"LOCATION:{EscapeIcsText(evt.Location)}");
            if (!string.IsNullOrEmpty(evt.Link)) sb.AppendLine($"URL:{evt.Link}");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }
        private string FormatDateToIcs(DateTime dt) => dt.ToString("yyyyMMddTHHmmssZ");
        private string EscapeIcsText(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\r\n", "\\n").Replace("\n", "\\n");
        private string StripHtml(string input) => string.IsNullOrEmpty(input) ? string.Empty : Regex.Replace(input, "<.*?>", String.Empty);
    }
}