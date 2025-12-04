using Astralis_BlazorApp.Services.Implementations;
using Astralis_BlazorApp.Services.Interfaces;

namespace Astralis_BlazorApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ITypeOfArticleService, TypeOfArticleService>();
        services.AddScoped<IStarService, StarService>();
        services.AddScoped<ISpectralClassService, SpectralClassService>();
        services.AddScoped<ISatelliteService, SatelliteService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportMotiveService, ReportMotiveService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPlanetTypeService, PlanetTypeService>();
        services.AddScoped<IPlanetService, PlanetService>();
        services.AddScoped<IPhonePrefixService, PhonePrefixService>();
        services.AddScoped<INotificationTypeService, NotificationTypeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IGalaxyQuasarClassService, GalaxyQuasarClassService>();
        services.AddScoped<IEventTypeService, EventTypeService>();
        // services.AddScoped<IEventInterestService, EventInterestService>();
        services.AddScoped<IDiscoveryStatusService, DiscoveryStatusService>();
        services.AddScoped<IDetectionMethodService, DetectionMethodService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICommandStatusService, CommandStatusService>();
        services.AddScoped<ICelestialBodyTypeService, CelestialBodyTypeService>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<IArticleTypeService, ArticleTypeService>();
        services.AddScoped<IAliasStatusService, AliasStatusService>();
        services.AddScoped<IAddressService, AddressService>();

        return services;
    }
}