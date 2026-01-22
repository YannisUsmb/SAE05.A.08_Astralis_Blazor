using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astralis_BlazorApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ICelestialBodyService _bodyService;
    private readonly IEventService _eventService;
    private readonly IArticleService _articleService;

    // Titre statique ou dynamique, au choix
    [ObservableProperty] private string title = "Explorez l’univers à travers les données";

    // Vraies données
    [ObservableProperty] private string countBodies = "-";
    [ObservableProperty] private string countEvents = "-";
    [ObservableProperty] private string countArticles = "-";
    [ObservableProperty] private string updateStatus = "En ligne";

    public HomeViewModel(
        ICelestialBodyService bodyService, 
        IEventService eventService, 
        IArticleService articleService)
    {
        _bodyService = bodyService;
        _eventService = eventService;
        _articleService = articleService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var bodiesTask = _bodyService.GetAllAsync();
            var eventsTask = _eventService.GetAllAsync();
            var articlesTask = _articleService.GetAllAsync();

            await Task.WhenAll(bodiesTask, eventsTask, articlesTask);

            var bodies = await bodiesTask;
            CountBodies = bodies.Count().ToString("N0");

            var events = await eventsTask;
            CountEvents = events.Count().ToString("N0");

            var articles = await articlesTask;
            CountArticles = articles.Count().ToString("N0");

            UpdateStatus = "Temps réel";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement stats accueil : {ex.Message}");
            CountBodies = "Err";
            UpdateStatus = "Hors ligne";
        }
    }
}