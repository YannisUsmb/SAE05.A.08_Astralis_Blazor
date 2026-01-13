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

        [ObservableProperty] private string typeSearchTerm = "";
        [ObservableProperty] private bool isTypeDropdownOpen;

        public IEnumerable<EventTypeDto> FilteredEventTypes => string.IsNullOrWhiteSpace(TypeSearchTerm)
            ? EventTypes
            : EventTypes.Where(t => t.Label.Contains(TypeSearchTerm, StringComparison.OrdinalIgnoreCase));

        [ObservableProperty] private bool isEditMode;
        [ObservableProperty] private int? eventId;
        [ObservableProperty] private bool isSubmitting;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private bool isUploadingImage;
        [ObservableProperty] private string? currentImageUrl;

        [ObservableProperty] private bool isTypeModalOpen;
        [ObservableProperty] private string newTypeName = "";
        [ObservableProperty] private string newTypeDescription = ""; 
        [ObservableProperty] private string? typeCreationError;

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
            if (!user.Identity?.IsAuthenticated ?? true) { _navigation.NavigateTo("/connexion"); return; }
            if (!user.IsInRole("Admin") && !user.IsInRole("Rédacteur Commercial")) { _navigation.NavigateTo("/evenements"); return; }

            await LoadEventTypesAsync();

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
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(1).AddHours(2)
                };
            }
        }

        private async Task LoadEventTypesAsync()
        {
            try
            {
                var types = await _eventTypeService.GetAllAsync();
                EventTypes = types.OrderBy(t => t.Label).ToList();
            }
            catch (Exception) { ErrorMessage = "Impossible de charger les types."; }
        }

        public void OpenCreateTypeModalFromSearch()
        {
            NewTypeName = TypeSearchTerm;
            NewTypeDescription = "";
            TypeCreationError = null;
            IsTypeDropdownOpen = false;
            IsTypeModalOpen = true;
        }

        public void CloseCreateTypeModal()
        {
            IsTypeModalOpen = false;
            NewTypeName = "";
            NewTypeDescription = "";
        }

        public async Task ConfirmCreateTypeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTypeName))
            {
                TypeCreationError = "Le nom est obligatoire.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewTypeDescription))
            {
                TypeCreationError = "La description est obligatoire.";
                return;
            }

            if (EventTypes.Any(t => t.Label.Equals(NewTypeName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                TypeCreationError = "Ce type existe déjà.";
                return;
            }

            try
            {
                // Création avec Description
                var dto = new EventTypeCreateDto
                {
                    Label = NewTypeName.Trim(),
                    Description = NewTypeDescription.Trim()
                };

                var createdType = await _eventTypeService.AddAsync(dto);

                EventTypes.Add(createdType);
                EventTypes = EventTypes.OrderBy(t => t.Label).ToList();

                EventModel.EventTypeId = createdType.Id;
                TypeSearchTerm = "";

                CloseCreateTypeModal();
            }
            catch (Exception ex)
            {
                TypeCreationError = $"Erreur : {ex.Message}";
            }
        }

        public void SelectType(int typeId)
        {
            EventModel.EventTypeId = typeId;
            IsTypeDropdownOpen = false;
            TypeSearchTerm = "";
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
                        StartDate = evtDto.StartDate.ToLocalTime(),
                        EndDate = evtDto.EndDate?.ToLocalTime(),
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
                    if (createdEvent != null) _navigation.NavigateTo($"/evenements/{createdEvent.Id}");
                    else _navigation.NavigateTo("/evenements");
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

        public void Cancel() => _navigation.NavigateTo("/evenements");
    }
}