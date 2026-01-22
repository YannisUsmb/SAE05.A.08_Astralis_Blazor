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
            // On lance les 3 requêtes en même temps pour gagner du temps
            var bodiesTask = _bodyService.GetAllAsync();
            var eventsTask = _eventService.GetAllAsync();
            var articlesTask = _articleService.GetAllAsync();

            await Task.WhenAll(bodiesTask, eventsTask, articlesTask);

            // Mise à jour des compteurs avec les vraies valeurs
            // Note : Idéalement, le backend devrait avoir une méthode "GetCountAsync" 
            // pour éviter de télécharger toute la liste, mais ceci fonctionne pour commencer.
            
            var bodies = await bodiesTask;
            CountBodies = bodies.Count().ToString("N0"); // "N0" ajoute des espaces (ex: 1 200)

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