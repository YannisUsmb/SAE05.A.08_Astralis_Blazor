using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventViewModel : ObservableObject
    {
        private readonly IEventService _eventService;
        private readonly IEventTypeService _eventTypeService;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ObservableCollection<EventDto> events = new();
        [ObservableProperty] private ObservableCollection<EventTypeDto> eventTypes = new();
        [ObservableProperty] private EventFilterDto filter = new() { PageSize = 9 };

        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string sortBy = "date_asc";

        [ObservableProperty] private string typeSearchTerm = "";

        public IEnumerable<EventTypeDto> FilteredEventTypes => string.IsNullOrWhiteSpace(TypeSearchTerm)
            ? EventTypes
            : EventTypes.Where(t => t.Label.Contains(TypeSearchTerm, StringComparison.OrdinalIgnoreCase));

        public ObservableCollection<int> SelectedTypeIds { get; set; } = new();

        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private bool hasNextPage;
        [ObservableProperty] private bool hasPreviousPage;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string eventCountMessage = "";

        private System.Timers.Timer _debounceTimer;

        public EventViewModel(
            IEventService eventService,
            IEventTypeService eventTypeService,
            NavigationManager navigation)
        {
            _eventService = eventService;
            _eventTypeService = eventTypeService;
            _navigation = navigation;

            _debounceTimer = new System.Timers.Timer(500);
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += async (sender, e) =>
            {
                await InvokeAsync(async () => await UpdateUrlAndSearch());
            };
        }

        private async Task InvokeAsync(Func<Task> workItem) => await workItem();

        public async Task InitializeAsync()
        {
            var types = await _eventTypeService.GetAllAsync();
            EventTypes = new ObservableCollection<EventTypeDto>(types);

            ParseUrlParameters();
            await SearchDataAsync();
        }

        private void ParseUrlParameters()
        {
            var uri = _navigation.ToAbsoluteUri(_navigation.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);

            if (query.TryGetValue("q", out var q)) SearchText = q!;
            if (query.TryGetValue("sort", out var sort)) SortBy = sort!;
            if (query.TryGetValue("page", out var page) && int.TryParse(page, out int p)) CurrentPage = p;

            SelectedTypeIds.Clear();
            if (query.TryGetValue("type", out var typeValues))
            {
                foreach (var val in typeValues)
                {
                    if (int.TryParse(val, out int id)) SelectedTypeIds.Add(id);
                }
            }
        }

        public async Task UpdateUrlAndSearch()
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["q"] = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                ["sort"] = SortBy == "date_asc" ? null : SortBy,
                ["page"] = CurrentPage <= 1 ? null : CurrentPage
            };

            if (SelectedTypeIds.Any())
            {
                queryParams["type"] = SelectedTypeIds.ToArray();
            }

            var uri = _navigation.GetUriWithQueryParameters(queryParams);
            _navigation.NavigateTo(uri, replace: true);

            await SearchDataAsync();
        }

        [RelayCommand]
        public async Task SearchDataAsync()
        {
            IsLoading = true;
            try
            {
                Filter.SearchText = SearchText;
                Filter.SortBy = SortBy;
                Filter.PageNumber = CurrentPage;
                Filter.EventTypeIds = SelectedTypeIds.Any() ? SelectedTypeIds.ToList() : null;

                var result = await _eventService.SearchAsync(Filter);

                Events = new ObservableCollection<EventDto>(result.Items);
                TotalPages = result.TotalPages;
                HasNextPage = result.HasNext;
                HasPreviousPage = result.HasPrevious;

                EventCountMessage = result.TotalCount > 0
                    ? $"{result.TotalCount} événement{(result.TotalCount > 1 ? "s" : "")} trouvé{(result.TotalCount > 1 ? "s" : "")}"
                    : "Aucun événement trouvé.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur recherche : {ex.Message}");
                Events.Clear();
                EventCountMessage = "Erreur de chargement.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ToggleEventType(int typeId)
        {
            if (typeId == 0)
            {
                SelectedTypeIds.Clear();
            }
            else
            {
                if (SelectedTypeIds.Contains(typeId))
                    SelectedTypeIds.Remove(typeId);
                else
                    SelectedTypeIds.Add(typeId);
            }

            CurrentPage = 1;
            await UpdateUrlAndSearch();
        }

        public void OnSearchInput(ChangeEventArgs e)
        {
            SearchText = e.Value?.ToString() ?? "";
            CurrentPage = 1;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public async Task OnSortChanged(ChangeEventArgs e)
        {
            SortBy = e.Value?.ToString() ?? "date_asc";
            CurrentPage = 1;
            await UpdateUrlAndSearch();
        }

        public async Task ChangePage(int newPage)
        {
            if (newPage < 1 || newPage > TotalPages || newPage == CurrentPage) return;
            CurrentPage = newPage;
            await UpdateUrlAndSearch();
        }
    }
}