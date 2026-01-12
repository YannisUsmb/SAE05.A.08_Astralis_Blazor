using Astralis_BlazorApp.Components;
using Astralis_BlazorApp.Extensions;
using Astralis_BlazorApp.Handlers;
using Astralis_BlazorApp.Services;
using Astralis_BlazorApp.ViewModels;
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
                client.BaseAddress = new Uri("https://localhost:7064/api/");
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