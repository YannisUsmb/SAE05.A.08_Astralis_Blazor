using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Implementations;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IUploadService _uploadService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDirty))]
        private UserUpdateDto profileData = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isUploading;

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private string? errorMessage;

        public EditContext? EditContext { get; set; }
        private ValidationMessageStore? _messageStore;

        private int _currentUserId;
        private UserUpdateDto _originalData = new();
        public bool IsDirty => !ProfileData.Equals(_originalData);

        public ProfileViewModel(
            IUserService userService,
            IUploadService uploadService,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation)
        {
            _userService = userService;
            _uploadService = uploadService;
            _authStateProvider = authStateProvider;
            _navigation = navigation;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var userIdStr = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdStr, out int userId) && userId > 0)
                {
                    _currentUserId = userId;
                    var userDto = await _userService.GetByIdAsync(userId);
                    if (userDto != null)
                    {
                        _originalData = new UserUpdateDto
                        {
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Username = userDto.Username,
                            Gender = userDto.Gender,
                            AvatarUrl = userDto.AvatarUrl,
                            Email = userDto.Email,
                            Phone = userDto.Phone,
                            CountryId = userDto.CountryId,
                            MultiFactorAuthentification = userDto.MultiFactorAuthentification
                        };

                        ProfileData = new UserUpdateDto
                        {
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Username = userDto.Username,
                            Gender = userDto.Gender,
                            AvatarUrl = userDto.AvatarUrl,
                            Email = userDto.Email,
                            Phone = userDto.Phone,
                            CountryId = userDto.CountryId,
                            MultiFactorAuthentification = userDto.MultiFactorAuthentification
                        };

                        InitializeEditContext();
                    }
                    }
                else
                {
                    ErrorMessage = "Session expirée. Veuillez vous reconnecter.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Impossible de charger le profil.";
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeEditContext()
        {
            EditContext = new EditContext(ProfileData);
            _messageStore = new ValidationMessageStore(EditContext);

            EditContext.OnFieldChanged += (s, e) => OnPropertyChanged(nameof(IsDirty));
            EditContext.OnValidationRequested += (s, e) => _messageStore.Clear();
        }

        public async Task UploadAvatarAsync(IBrowserFile file)
        {
            if (file == null) return;
            IsUploading = true;
            ErrorMessage = null;
            try
            {
                if (file.Size > 5 * 1024 * 1024)
                {
                    ErrorMessage = "L'image est trop volumineuse (max 5Mo).";
                    return;
                }

                var url = await _uploadService.UploadImageAsync(file, UploadCategory.Avatars);

                if (!string.IsNullOrEmpty(url))
                {
                    ProfileData.AvatarUrl = url;
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
            catch
            {
                ErrorMessage = "Erreur lors de l'upload.";
            }
            finally
            {
                IsUploading = false;
            }
        }

        public async Task ValidateUsernameAvailabilityAsync()
        {
            if (string.IsNullOrWhiteSpace(ProfileData.Username) ||
                ProfileData.Username.Equals(_originalData.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (EditContext != null)
                {
                    _messageStore?.Clear(EditContext.Field(nameof(ProfileData.Username)));
                    EditContext.NotifyValidationStateChanged();
                }
                return;
            }

            var availability = await _userService.CheckAvailabilityAsync(null, ProfileData.Username, null, null);

            if (EditContext != null)
            {
                var field = EditContext.Field(nameof(ProfileData.Username));
                _messageStore?.Clear(field);

                if (availability != null && availability.IsTaken)
                {
                    _messageStore?.Add(field, availability.Message ?? "Ce pseudo est déjà pris.");
        }

                EditContext.NotifyValidationStateChanged();
            }
        }

        public async Task SaveChangesAsync(bool silent = false, bool forceReload = false)
        {
            if (IsLoading) return;

            if (!silent && (EditContext == null || !EditContext.Validate())) return;

            IsLoading = true;
            SuccessMessage = null;
            ErrorMessage = null;
            _messageStore?.Clear();

            try
            {
                if (ProfileData.Username != _originalData.Username)
                {
                    var availability = await _userService.CheckAvailabilityAsync(null, ProfileData.Username, null, null);
                    if (availability != null && availability.IsTaken)
                    {
                        _messageStore?.Add(EditContext!.Field(nameof(ProfileData.Username)), availability.Message ?? "Pris.");
                        EditContext!.NotifyValidationStateChanged();
                        IsLoading = false;
                        return;
                    }
                }

                ProfileData.Email = _originalData.Email;
                ProfileData.Phone = _originalData.Phone;
                ProfileData.CountryId = _originalData.CountryId;
                ProfileData.MultiFactorAuthentification = _originalData.MultiFactorAuthentification;

                await _userService.UpdateAsync(_currentUserId, ProfileData);

                _originalData = new UserUpdateDto
                {
                    FirstName = ProfileData.FirstName,
                    LastName = ProfileData.LastName,
                    Username = ProfileData.Username,
                    Gender = ProfileData.Gender,
                    AvatarUrl = ProfileData.AvatarUrl,
                    Email = ProfileData.Email,
                    Phone = ProfileData.Phone,
                    CountryId = ProfileData.CountryId,
                    MultiFactorAuthentification = ProfileData.MultiFactorAuthentification
                };

                OnPropertyChanged(nameof(IsDirty));

                if (!silent) SuccessMessage = "Profil mis à jour avec succès.";

                if (_authStateProvider is CustomAuthProvider customAuth)
                {
                    await customAuth.RefreshUserSession();
                }

                if (forceReload) _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur lors de la sauvegarde.";
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}