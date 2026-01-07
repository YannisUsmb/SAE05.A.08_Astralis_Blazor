using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Astralis_BlazorApp.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const string Controller = "Auth";
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResponseDto?> Login(UserLoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{Controller}/Login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var userResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (_authStateProvider is CustomAuthProvider customProvider)
                {
                    customProvider.MarkUserAsAuthenticated(userResult!);
                }

                return userResult;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception(errorResponse.Trim('"'));
            }

            throw new Exception("Une erreur technique est survenue sur le serveur.");
        }

        public async Task<AuthResponseDto?> GoogleLogin(GoogleLoginDto googleDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{Controller}/GoogleLogin", googleDto);

            if (response.IsSuccessStatusCode)
            {
                var userResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (_authStateProvider is CustomAuthProvider customProvider)
                {
                    customProvider.MarkUserAsAuthenticated(userResult!);
                }

                return userResult;
            }

            return null;
        }

        public async Task Logout()
        {
            await _httpClient.PostAsync($"{Controller}/Logout", null);

            if (_authStateProvider is CustomAuthProvider customProvider)
            {
                customProvider.MarkUserAsLoggedOut();
            }
        }

        public async Task<bool> CheckUserSession()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{Controller}/Me");

                if (response.IsSuccessStatusCode)
                {
                    var userResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                    if (_authStateProvider is CustomAuthProvider customProvider)
                    {
                        customProvider.MarkUserAsAuthenticated(userResult!);
                    }
                    return true;
                }
            }
            catch
            {
                // Silent management: the user is simply not logged in.
            }

            return false;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/Verify-Email", token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var dto = new ForgotPasswordDto { Email = email };
            var response = await _httpClient.PostAsJsonAsync($"{Controller}/Forgot-Password", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{Controller}/Reset-Password", dto);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception(error.Trim('"'));
            }
            return true;
        }
    }
}