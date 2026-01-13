using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Astralis_BlazorApp.ViewModels
{
    public class PremiumViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        public bool IsLoading { get; private set; } = true;
        public bool IsUserPremium { get; private set; } = false;
        public bool IsYearly { get; set; } = false;
        public string ErrorMessage { get; private set; } = string.Empty;

        public PremiumViewModel(HttpClient httpClient, NavigationManager navigation, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    var uri = _navigation.ToAbsoluteUri(_navigation.Uri);
                    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("session_id", out var sessionId))
                    {
                        await _httpClient.GetAsync($"payment/verify-premium?session_id={sessionId}");
                        _navigation.NavigateTo("/premium", replace: true);
                    }

                    var response = await _httpClient.GetAsync("payment/sync-status");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<SyncStatusResponse>();
                        IsUserPremium = result?.IsPremium ?? false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Init Premium: {ex.Message}");
                ErrorMessage = "Impossible de récupérer le statut.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void TogglePlan() => IsYearly = !IsYearly;

        public void GoToLogin() => _navigation.NavigateTo("/login");

        public async Task SubscribeToPremium()
        {
            IsLoading = true;
            try
            {
                var payload = new { planType = IsYearly ? "yearly" : "monthly" };
                var response = await _httpClient.PostAsJsonAsync("payment/subscribe", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<StripeUrlResponse>();
                    if (!string.IsNullOrEmpty(result?.Url))
                    {
                        _navigation.NavigateTo(result.Url, forceLoad: true);
                    }
                }
                else
                {
                    ErrorMessage = "Erreur lors de la création de la session de paiement.";
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                IsLoading = false;
            }
        }

        public async Task GoToStripePortal()
        {
            IsLoading = true;
            try
            {
                var response = await _httpClient.PostAsync("payment/portal", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<StripeUrlResponse>();
                    if (!string.IsNullOrEmpty(result?.Url))
                    {
                        _navigation.NavigateTo(result.Url, forceLoad: true);
                    }
                }
                else
                {
                    ErrorMessage = "Impossible d'accéder au portail.";
                    IsLoading = false;
                }
            }
            catch
            {
                IsLoading = false;
            }
        }

        private class SyncStatusResponse { public bool IsPremium { get; set; } }
        private class StripeUrlResponse { public string Url { get; set; } }
    }
}