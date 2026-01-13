using Astralis_BlazorApp.Handlers;
using Astralis_BlazorApp.Services;
using Astralis_BlazorApp.Services.Implementations;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis_BlazorApp.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;

namespace Astralis_BlazorApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        services.AddScoped<IUserNotificationTypeService, UserNotificationTypeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITypeOfArticleService, TypeOfArticleService>();
        services.AddScoped<IStarService, StarService>();
        services.AddScoped<ISpectralClassService, SpectralClassService>();
        services.AddScoped<ISatelliteService, SatelliteService>();
        services.AddScoped<IReportStatusService, ReportStatusService>();
        services.AddScoped<IReportMotiveService, ReportMotiveService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPlanetTypeService, PlanetTypeService>();
        services.AddScoped<IPlanetService, PlanetService>();
        services.AddScoped<IPhonePrefixService, PhonePrefixService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IOrbitalClassService, OrbitalClassService>();
        services.AddScoped<INotificationTypeService, NotificationTypeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IGalaxyQuasarClassService, GalaxyQuasarClassService>();
        services.AddScoped<IGalaxyQuasarService, GalaxyQuasarService>();
        services.AddScoped<IEventTypeService, EventTypeService>();
        services.AddScoped<IEventInterestService, EventInterestService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IDiscoveryStatusService, DiscoveryStatusService>();
        services.AddScoped<IDiscoveryService, DiscoveryService>();
        services.AddScoped<IDetectionMethodService, DetectionMethodService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICommandStatusService, CommandStatusService>();
        services.AddScoped<ICommandService, CommandService>();
        services.AddScoped<ICometService, CometService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<ICelestialBodyTypeService, CelestialBodyTypeService>();
        services.AddScoped<ICelestialBodyService, CelestialBodyService>();
        services.AddScoped<ICartItemService, CartItemService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<IAsteroidService, AsteroidService>();
        services.AddScoped<IArticleTypeService, ArticleTypeService>();
        services.AddScoped<IArticleInterestService, ArticleInterestService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IAliasStatusService, AliasStatusService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
        services.AddTransient<HttpResponseHandler>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IUploadService, UploadService>();

        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddScoped<MainLayoutViewModel>();
        services.AddScoped<HomeViewModel>();
        services.AddScoped<CelestialBodyViewModel>();
        services.AddScoped<LoginViewModel>();
        services.AddScoped<SignUpViewModel>();
        services.AddScoped<EventViewModel>();
        services.AddScoped<EventDetailsViewModel>();
        services.AddScoped<EventEditorViewModel>();
        services.AddScoped<AccountViewModel>();
        services.AddScoped<ProfileViewModel>();
        services.AddScoped<ArticleListViewModel>();
        services.AddScoped<ArticleEditorViewModel>();
        services.AddScoped<ArticleDetailsViewModel>();
        services.AddScoped<ProductEditorViewModel>();
        services.AddScoped<ShopViewModel>();
        services.AddScoped<AddressViewModel>();
        return services;
    }
}