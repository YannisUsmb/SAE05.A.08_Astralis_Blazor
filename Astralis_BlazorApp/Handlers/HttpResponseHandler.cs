using Astralis_BlazorApp.Extensions;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace Astralis_BlazorApp.Handlers
{
    public class HttpResponseHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;

        public HttpResponseHandler(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (!_navigationManager.Uri.Contains("/connexion"))
                {
                    _navigationManager.NavigateToLogin();
                }
            }

            return response;
        }
    }
}