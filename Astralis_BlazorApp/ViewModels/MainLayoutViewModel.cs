using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Astralis_BlazorApp.ViewModels
{
    public class MainLayoutViewModel
    {
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigationManager;

        public MainLayoutViewModel(IAuthService authService, NavigationManager navigationManager)
        {
            _authService = authService;
            _navigationManager = navigationManager;
        }

        public async Task LogoutAsync()
        {
            await _authService.Logout();

            _navigationManager.NavigateTo("connexion", forceLoad: true);
        }
    }
}