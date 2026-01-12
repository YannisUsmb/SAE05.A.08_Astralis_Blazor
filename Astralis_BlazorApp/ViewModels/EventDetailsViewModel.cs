using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.RegularExpressions;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventDetailsViewModel : ObservableObject
    {
        private readonly IEventService _eventService;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        private EventDto? selectedEvent;

        [ObservableProperty]
        private bool isLoading = true;

        public EventDetailsViewModel(
            IEventService eventService,
            NavigationManager navigation,
            IJSRuntime jsRuntime)
        {
            _eventService = eventService;
            _navigation = navigation;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(int eventId)
        {
            IsLoading = true;
            try
            {
                SelectedEvent = await _eventService.GetByIdAsync(eventId);
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

        public async Task AddToCalendar()
        {
            if (SelectedEvent == null) return;

            string icsContent = GenerateIcsContent(SelectedEvent);

            string fileName = $"astralis-event-{SelectedEvent.Id}.ics";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "text/calendar", icsContent);
        }

        public void ToggleInterest()
        {
            Console.WriteLine("Toggle intérêt...");
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

            if (!string.IsNullOrEmpty(evt.Location))
            {
                sb.AppendLine($"LOCATION:{EscapeIcsText(evt.Location)}");
            }

            if (!string.IsNullOrEmpty(evt.Link))
            {
                sb.AppendLine($"URL:{evt.Link}");
            }

            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        private string FormatDateToIcs(DateTime dt) => dt.ToString("yyyyMMddTHHmmssZ");

        private string EscapeIcsText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}