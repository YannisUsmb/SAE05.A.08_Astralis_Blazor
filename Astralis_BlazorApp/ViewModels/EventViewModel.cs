using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventViewModel : ObservableObject
    {
        private readonly IEventService _eventService;

        [ObservableProperty] private ObservableCollection<EventDto> events = new();
        [ObservableProperty] private EventFilterDto filter = new();

        // Filtres UI
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string sortBy = "date_asc";

        // Pagination
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int pageSize = 9;
        [ObservableProperty] private bool hasNextPage = true;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string eventCountMessage = "";

        // Modale
        [ObservableProperty] private bool isModalVisible;
        [ObservableProperty] private EventDto? selectedEvent;

        public EventViewModel(IEventService eventService)
        {
            _eventService = eventService;
        }

        [RelayCommand]
        public async Task InitializeAsync()
        {
            await SearchDataAsync();
        }

        [RelayCommand]
        public async Task SearchDataAsync()
        {
            IsLoading = true;
            try
            {
                Filter.SearchText = SearchText;

                var results = await _eventService.SearchAsync(Filter, CurrentPage, PageSize, SortBy);

                HasNextPage = results.Count == PageSize;

                EventCountMessage = results.Count > 0
                    ? $"{results.Count} événement{(results.Count > 1 ? "s" : "")} sur cette page"
                    : "Aucun événement à venir 🌌";

                Events = new ObservableCollection<EventDto>(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur recherche événements : {ex.Message}");
                Events.Clear();
                EventCountMessage = "Erreur de chargement.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnFilterChanged()
        {
            CurrentPage = 1;
            await SearchDataAsync();
        }

        [RelayCommand]
        public void OpenDetails(EventDto evt)
        {
            SelectedEvent = evt;
            IsModalVisible = true;
        }

        [RelayCommand]
        public void CloseDetails()
        {
            IsModalVisible = false;
            SelectedEvent = null;
        }

        [RelayCommand]
        public async Task NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                await SearchDataAsync();
            }
        }

        [RelayCommand]
        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await SearchDataAsync();
            }
        }
    }
}