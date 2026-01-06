using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private UserUpdateDto profileData = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isUploading;

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private string? errorMessage;

        private ValidationMessageStore? _messageStore;
        public EditContext? EditContext { get; set; }

        private int _currentUserId;

        private string _originalUsername = "";
        private string _originalEmail = "";
        private string? _originalPhone;
        private int? _originalCountryId;
        private bool _originalMfa;

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

                if (int.TryParse(userIdStr, out int userId))
                {
                    _currentUserId = userId;
                    var userDto = await _userService.GetByIdAsync(userId);
                    if (userDto != null)
                    {
                        _originalUsername = userDto.Username;
                        _originalEmail = userDto.Email;
                        _originalPhone = userDto.Phone;
                        _originalCountryId = userDto.CountryId;
                        _originalMfa = userDto.MultiFactorAuthentification;

                        ProfileData = new UserUpdateDto
                        {
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Username = userDto.Username,
                            Gender = userDto.Gender,
                            AvatarUrl = userDto.AvatarUrl,
                            Email = userDto.Email,
                            Phone = userDto.Phone,
                            CountryId = userDto.CountryId
                        };
                    }
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

                var url = await _uploadService.UploadImageAsync(file);
                if (!string.IsNullOrEmpty(url))
                {
                    ProfileData.AvatarUrl = url;
                    await SaveChangesAsync(silent: true, forceRefresh: true);
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

        public async Task SaveChangesAsync(bool silent = false, bool forceRefresh = false)
        {
            if (IsLoading) return;
            IsLoading = true;
            SuccessMessage = null;
            ErrorMessage = null;
            _messageStore?.Clear();

            if (EditContext != null && !EditContext.Validate())
            {
                IsLoading = false;
                return;
            }

            try
            {
                if (ProfileData.Username != _originalUsername)
                {
                    var availability = await _userService.CheckAvailabilityAsync(null, ProfileData.Username, null, null);

                    if (availability != null && availability.IsTaken)
                    {
                        if (EditContext != null)
                        {
                            _messageStore?.Add(EditContext.Field(nameof(ProfileData.Username)), availability.Message ?? "Ce pseudo est déjà pris.");
                            EditContext.NotifyValidationStateChanged();
                        }
                        IsLoading = false;
                        return;
                    }
                }

                ProfileData.Email = _originalEmail;
                ProfileData.Phone = _originalPhone;
                ProfileData.CountryId = _originalCountryId;
                ProfileData.MultiFactorAuthentification = _originalMfa;

                await _userService.UpdateAsync(_currentUserId, ProfileData);

                _originalUsername = ProfileData.Username;

                if (!silent) SuccessMessage = "Profil mis à jour avec succès.";

                if (forceRefresh) _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
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