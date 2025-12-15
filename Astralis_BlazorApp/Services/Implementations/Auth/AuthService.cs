using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace Astralis_BlazorApp.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponseDto?> Login(UserLoginDto loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var userResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            ((CustomAuthProvider)_authStateProvider).MarkUserAsAuthenticated(userResult);

            return userResult;
        }

        return null;
    }

    public async Task Logout()
    {
        await _httpClient.PostAsync("api/Auth/Logout", null);

        ((CustomAuthProvider)_authStateProvider).MarkUserAsLoggedOut();
    }

    public async Task<bool> CheckUserSession()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Auth/Me");

            if (response.IsSuccessStatusCode)
            {
                var userResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                ((CustomAuthProvider)_authStateProvider).MarkUserAsAuthenticated(userResult);
                return true;
            }
        }
        catch
        {
        }

        return false;
    }
}