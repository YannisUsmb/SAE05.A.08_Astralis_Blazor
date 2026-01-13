using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventViewModel : ObservableObject
    {
        private readonly IEventService _eventService;
        private readonly IEventTypeService _eventTypeService;
        private readonly IEventInterestService _interestService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ObservableCollection<EventDto> events = new();
        [ObservableProperty] private ObservableCollection<EventTypeDto> eventTypes = new();
        [ObservableProperty] private EventFilterDto filter = new() { PageSize = 9 };

        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string sortBy = "date_asc";
        [ObservableProperty] private string typeSearchTerm = "";

        [ObservableProperty] private bool canCreate;
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private int currentUserId;

        [ObservableProperty] private EventDto? eventToDelete;
        [ObservableProperty] private bool isDeleteModalOpen;

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
            IEventInterestService interestService,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation)
        {
            _eventService = eventService;
            _eventTypeService = eventTypeService;
            _interestService = interestService;
            _authStateProvider = authStateProvider;
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
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                IsAdmin = user.IsInRole("Admin");
                CanCreate = IsAdmin || user.IsInRole("Rédacteur Commercial");

                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int uid))
                {
                    CurrentUserId = uid;
                }
            }

            var types = await _eventTypeService.GetAllAsync();
            EventTypes = new ObservableCollection<EventTypeDto>(types);

            ParseUrlParameters();
            await SearchDataAsync();
        }

        // --- NAVIGATION ---
        [RelayCommand]
        public void NavigateToCreate()
        {
            _navigation.NavigateTo("/admin/evenements/nouveau");
        }

        [RelayCommand]
        public void NavigateToEdit(int id)
        {
            _navigation.NavigateTo($"/admin/evenements/edition/{id}");
        }

        public void OpenDeleteModal(EventDto evt)
        {
            EventToDelete = evt;
            IsDeleteModalOpen = true;
        }

        public void CloseDeleteModal()
        {
            IsDeleteModalOpen = false;
            EventToDelete = null;
        }

        [RelayCommand]
        public async Task ConfirmDeleteAsync()
        {
            if (EventToDelete == null) return;

            try
            {
                await _eventService.DeleteAsync(EventToDelete.Id);

                // On retire l'élément de la liste locale pour éviter de tout recharger
                Events.Remove(EventToDelete);

                // Mettre à jour le compteur (optionnel)
                EventCountMessage = "Événement supprimé.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression: {ex.Message}");
            }
            finally
            {
                CloseDeleteModal();
            }
        }

        // ... (Le reste : ParseUrlParameters, UpdateUrlAndSearch, SearchDataAsync, ToggleInterestAsync, etc. reste inchangé) ...

        // RAPPEL : Remets bien tes méthodes existantes ici (SearchDataAsync, etc.) que je n'ai pas recopiées pour faire court.
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

        [RelayCommand]
        public async Task ToggleInterestAsync(EventDto evt)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            bool wasInterested = evt.IsInterested;
            evt.IsInterested = !evt.IsInterested;

            if (evt.IsInterested) evt.InterestCount++;
            else evt.InterestCount--;

            OnPropertyChanged(nameof(Events));

            try
            {
                if (wasInterested)
                    await _interestService.DeleteAsync(evt.Id, CurrentUserId);
                else
                    await _interestService.AddAsync(new EventInterestDto { EventId = evt.Id, UserId = CurrentUserId });
            }
            catch (Exception)
            {
                evt.IsInterested = wasInterested;
                if (evt.IsInterested) evt.InterestCount++; else evt.InterestCount--;
                OnPropertyChanged(nameof(Events));
            }
        }

        public async Task ToggleEventType(int typeId)
        {
            if (typeId == 0) SelectedTypeIds.Clear();
            else if (SelectedTypeIds.Contains(typeId)) SelectedTypeIds.Remove(typeId);
            else SelectedTypeIds.Add(typeId);

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