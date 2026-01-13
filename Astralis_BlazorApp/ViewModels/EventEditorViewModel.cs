using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class EventEditorViewModel : ObservableObject
    {
        private readonly IEventService _eventService;
        private readonly IEventTypeService _eventTypeService;
        private readonly IUploadService _uploadService;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        [ObservableProperty] private EventCreateDto eventModel = new();
        [ObservableProperty] private List<EventTypeDto> eventTypes = new();

        [ObservableProperty] private bool isEditMode;
        [ObservableProperty] private int? eventId;
        [ObservableProperty] private bool isSubmitting;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private bool isUploadingImage;

        [ObservableProperty] private string? currentImageUrl;

        public EventEditorViewModel(
            IEventService eventService,
            IEventTypeService eventTypeService,
            IUploadService uploadService,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider)
        {
            _eventService = eventService;
            _eventTypeService = eventTypeService;
            _uploadService = uploadService;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync(int? id)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _navigation.NavigateTo("/connexion");
                return;
            }

            bool canEdit = user.IsInRole("Admin") || user.IsInRole("Rédacteur Commercial");
            if (!canEdit)
            {
                _navigation.NavigateTo("/evenements");
                return;
            }

            try
            {
                EventTypes = await _eventTypeService.GetAllAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de charger les types d'événements.";
                Console.WriteLine(ex);
            }

            if (id.HasValue && id.Value > 0)
            {
                IsEditMode = true;
                EventId = id.Value;
                await LoadEventForEdit(id.Value);
            }
            else
            {
                IsEditMode = false;
                EventModel = new EventCreateDto
                {
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(1).AddHours(2)
                };
            }
        }

        private async Task LoadEventForEdit(int id)
        {
            try
            {
                var evtDto = await _eventService.GetByIdAsync(id);
                if (evtDto != null)
                {
                    EventModel = new EventCreateDto
                    {
                        Title = evtDto.Title,
                        Description = evtDto.Description,
                        StartDate = evtDto.StartDate,
                        EndDate = evtDto.EndDate,
                        Location = evtDto.Location,
                        Link = evtDto.Link,
                        PictureUrl = evtDto.PictureUrl,
                        EventTypeId = evtDto.EventTypeId
                    };
                    CurrentImageUrl = evtDto.PictureUrl;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors du chargement : {ex.Message}";
            }
        }

        public async Task UploadImageAsync(IBrowserFile file)
        {
            if (file == null) return;

            IsUploadingImage = true;
            ErrorMessage = null;

            try
            {
                if (file.Size > 5 * 1024 * 1024)
                {
                    ErrorMessage = "L'image est trop volumineuse (Max 5MB).";
                    return;
                }

                string uploadedUrl = await _uploadService.UploadImageAsync(file, UploadCategory.Events);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    EventModel.PictureUrl = uploadedUrl;
                    CurrentImageUrl = uploadedUrl;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur d'upload : {ex.Message}";
            }
            finally
            {
                IsUploadingImage = false;
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(EventModel.Title))
            {
                ErrorMessage = "Le titre est obligatoire.";
                return;
            }
            if (EventModel.EventTypeId == 0)
            {
                ErrorMessage = "Veuillez sélectionner un type d'événement.";
                return;
            }
            if (EventModel.EndDate < EventModel.StartDate)
            {
                ErrorMessage = "La date de fin ne peut pas être avant la date de début.";
                return;
            }

            IsSubmitting = true;
            ErrorMessage = null;

            try
            {
                if (IsEditMode && EventId.HasValue)
                {
                    var updateDto = new EventUpdateDto
                    {
                        Title = EventModel.Title,
                        Description = EventModel.Description,
                        StartDate = EventModel.StartDate,
                        EndDate = EventModel.EndDate,
                        Location = EventModel.Location,
                        Link = EventModel.Link,
                        PictureUrl = EventModel.PictureUrl,
                        EventTypeId = EventModel.EventTypeId
                    };

                    await _eventService.UpdateAsync(EventId.Value, updateDto);
                    _navigation.NavigateTo($"/evenements/{EventId.Value}");
                }
                else
                {
                    var createdEvent = await _eventService.AddAsync(EventModel);
                    if (createdEvent != null)
                    {
                        _navigation.NavigateTo($"/evenements/{createdEvent.Id}");
                    }
                    else
                    {
                        _navigation.NavigateTo("/evenements");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de l'enregistrement : {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        public void Cancel()
        {
            _navigation.NavigateTo("/evenements");
        }
    }
}