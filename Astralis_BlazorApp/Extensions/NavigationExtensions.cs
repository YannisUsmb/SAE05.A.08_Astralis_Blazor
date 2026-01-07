using Microsoft.AspNetCore.Components;
using System.Net;

namespace Astralis_BlazorApp.Extensions
{
    public static class NavigationExtensions
    {
        public static void NavigateToLogin(this NavigationManager nav, string returnUrl = "")
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = nav.Uri;
            }

            var encodedReturnUrl = WebUtility.UrlEncode(returnUrl);
            nav.NavigateTo($"/connexion?returnUrl={encodedReturnUrl}", forceLoad: false);
        }
    }
}