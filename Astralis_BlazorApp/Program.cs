using Astralis_BlazorApp.Components;
using Astralis_BlazorApp.Extensions;
using Astralis_BlazorApp.Handlers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Astralis_BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // --- HTTP Configuration & Cookies ---
            builder.Services.AddTransient<CookieHandler>();

            builder.Services.AddHttpClient("AstralisAPI", client =>
            {
                string? apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrEmpty(apiUrl))
                {
                    throw new Exception("L'URL de l'API n'est pas configurée dans appsettings.json (ApiSettings:BaseUrl)");
                }

                client.BaseAddress = new Uri(apiUrl);
            })
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<HttpResponseHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AstralisAPI"));

            // --- Services & ViewModels ---
            builder.Services.AddApplicationServices();
            builder.Services.AddViewModels();

            // --- Authorization & Auth ---
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("MustBePremium", policy =>
                    policy.RequireClaim("IsPremium", "true"));
            });

            builder.Services.AddBlazorBootstrap();

            await builder.Build().RunAsync();
        }
    }
}