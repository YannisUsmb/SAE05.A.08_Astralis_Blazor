using Astralis_BlazorApp.Components;
using Astralis_BlazorApp.Extensions;
using Astralis_BlazorApp.Services.Implementations;
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

            builder.Services.AddScoped(sp =>
            {
                var handler = new CookieHandler();
                handler.InnerHandler = new HttpClientHandler();

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://localhost:7064/api/")
                };

                return client;
            });

            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<CelestialBodyViewModel>();
            builder.Services.AddScoped<LoginViewModel>();
            builder.Services.AddScoped<MainLayoutViewModel>();
            builder.Services.AddScoped<SignUpViewModel>();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("MustBePremium", policy =>
                    policy.RequireClaim("IsPremium", "true"));
            });

            

            await builder.Build().RunAsync();
        }
    }
}
