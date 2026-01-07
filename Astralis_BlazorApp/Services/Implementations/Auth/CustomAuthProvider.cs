using System.Net.Http.Json;
using System.Security.Claims;
using Astralis.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        private bool _isInitialized = false;

        public CustomAuthProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_isInitialized)
            {
                return new AuthenticationState(_currentUser);
            }

            try
            {
                var userDto = await _httpClient.GetFromJsonAsync<AuthResponseDto>("Auth/Me");

                if (userDto != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                        new Claim(ClaimTypes.Name, userDto.Username),
                        new Claim(ClaimTypes.Role, userDto.Role),
                        new Claim("AvatarPath", userDto.AvatarUrl ?? string.Empty),
                        new Claim("IsPremium", userDto.IsPremium ? "true" : "false")
                    };

                    var identity = new ClaimsIdentity(claims, "ServerAuth");
                    _currentUser = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
            }
            finally
            {
                _isInitialized = true;
            }

            return new AuthenticationState(_currentUser);
        }

        public void MarkUserAsAuthenticated(AuthResponseDto userDto)
        {
            _isInitialized = true;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Name, userDto.Username),
                new Claim(ClaimTypes.Role, userDto.Role),
                new Claim("AvatarPath", userDto.AvatarUrl ?? string.Empty),
                new Claim("IsPremium", userDto.IsPremium ? "true" : "false")
            };

            var identity = new ClaimsIdentity(claims, "apiauth");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            _isInitialized = true;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task RefreshUserSession()
        {
            try
            {
                var userDto = await _httpClient.GetFromJsonAsync<AuthResponseDto>("Auth/Me");
                if (userDto != null)
                {
                    MarkUserAsAuthenticated(userDto);
                }
            }
            catch
            {
                MarkUserAsLoggedOut();
            }
        }
    }
}