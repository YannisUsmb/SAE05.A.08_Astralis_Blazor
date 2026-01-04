using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Astralis_BlazorApp.ViewModels
{
    public class AccountViewModel
    {
        private readonly IUserService _userService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;

        public string CurrentEmail { get; set; } = "Chargement...";
        public string NewEmail { get; set; } = "";

        public ChangePasswordDto PasswordModel { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public AccountViewModel(IUserService userService, AuthenticationStateProvider authStateProvider, NavigationManager navigation)
        {
            _userService = userService;
            _authStateProvider = authStateProvider;
            _navigation = navigation;
        }

        public async Task LoadUserDataAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                Console.WriteLine("Utilisateur non authentifié dans le ViewModel");
                CurrentEmail = "Non connecté.";
                return;
            }

            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine("ERREUR CRITIQUE: Claim NameIdentifier introuvable !");
                CurrentEmail = "Erreur: ID utilisateur manquant.";
                return;
            }

            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                var userDto = await _userService.GetByIdAsync(userId);
                if (userDto != null)
                {
                    CurrentEmail = userDto.Email;
                    NewEmail = userDto.Email;
                    return;
                }
            }

            CurrentEmail = "Impossible de charger les données.";
        }

        public async Task UpdateEmailAsync()
        {
            SuccessMessage = null;
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(NewEmail) || NewEmail == CurrentEmail)
            {
                return;
            }

            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var userIdStr = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdStr, out int userId))
                {
                    var userDto = await _userService.GetByIdAsync(userId);
                    if (userDto != null)
                    {
                        var updateDto = new UserUpdateDto
                        {
                            Email = NewEmail,
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Username = userDto.Username,
                            UserAvatarUrl = userDto.UserAvatarUrl,
                            Gender = userDto.Gender,
                            MultiFactorAuthentification = userDto.MultiFactorAuthentification,
                            Phone = userDto.Phone
                        };

                        await _userService.UpdateAsync(userId, updateDto);
                        CurrentEmail = NewEmail;
                        SuccessMessage = "Adresse email mise à jour avec succès.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR UpdateEmail] : {ex.Message}");

                ErrorMessage = "Une erreur est survenue lors de la mise à jour de votre email. Veuillez réessayer plus tard.";
            }
        }

        public async Task ChangePasswordAsync()
        {
            SuccessMessage = null;
            ErrorMessage = null;

            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var userIdStr = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdStr, out int userId))
                {
                    await _userService.ChangePasswordAsync(userId, PasswordModel);
                    SuccessMessage = "Mot de passe modifié avec succès.";
                    PasswordModel = new ChangePasswordDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR ChangePassword] : {ex.Message}");

                ErrorMessage = "Impossible de modifier le mot de passe. Vérifiez votre mot de passe actuel ou réessayez plus tard.";
            }
        }
    }
}