using Astralis_BlazorApp.Components;
using Astralis_BlazorApp.Extensions;
using Astralis_BlazorApp.Services.Implementations;
using Astralis_BlazorApp.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
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

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7064/api/")
            });
            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<CelestialBodyViewModel>();
            builder.Services.AddScoped<LoginViewModel>();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthProvider>());

            await builder.Build().RunAsync();
        }
    }
}
