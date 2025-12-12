using System.Collections.ObjectModel;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astralis_BlazorApp.ViewModels;

public partial class CelestialBodyViewModel : ObservableObject
{
    private readonly ICelestialBodyService _bodyService;
    private readonly ICelestialBodyTypeService _typeService;

    // Données pour la vue
    [ObservableProperty] private ObservableCollection<CelestialBodyListDto> celestialBodies = new();
    [ObservableProperty] private ObservableCollection<CelestialBodyTypeDto> celestialBodyTypes = new();

    // Le Filtre
    [ObservableProperty] private CelestialBodyFilterDto filter = new();

    // Gestion de la sélection
    [ObservableProperty] private int selectedTypeId = 0;

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    // Gestion de l'UI
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private CelestialBodyListDto? selectedBody;


    public CelestialBodyViewModel(ICelestialBodyService bodyService, ICelestialBodyTypeService typeService)
    {
        _bodyService = bodyService;
        _typeService = typeService;
    }

    // --- 1. Initialisation ---
    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            // Charger les types pour le menu déroulant
            var types = await _typeService.GetAllAsync();
            CelestialBodyTypes = new ObservableCollection<CelestialBodyTypeDto>(types);
            // Charger la liste initiale
            await SearchDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur init : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- 2. Recherche ---
    [RelayCommand]
    public async Task SearchDataAsync()
    {
        IsLoading = true;
        try
        {
            // Mapping du filtre (inchangé)
            Filter.CelestialBodyTypeIds = SelectedTypeId != 0 ? new List<int> { SelectedTypeId } : null;

            // --- CHANGEMENT ICI : On doit passer la page au service ---
            // Note : Ton service doit accepter (Filter, PageNumber, PageSize)
            var results = await _bodyService.SearchAsync(Filter, CurrentPage, PageSize);
            
            // Logique simple : Si on reçoit moins d'items que demandé, c'est la dernière page
            HasNextPage = results.Count == PageSize;

            var sortedResults = results.OrderBy(x => x.Name).ToList();
            CelestialBodies = new ObservableCollection<CelestialBodyListDto>(sortedResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur recherche : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- 3. Navigation ---
    [RelayCommand]
    public void ShowDetails(CelestialBodyListDto body)
    {
        SelectedBody = body;
        Is3DVisible = true;
        // Pas fini
    }

    [RelayCommand]
    public void BackToList()
    {
        Is3DVisible = false;
        SelectedBody = null;
    }
    
    [RelayCommand]
    public async Task NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            await SearchDataAsync();
        }
    }

    [RelayCommand]
    public async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await SearchDataAsync();
        }
    }
    
    public async Task OnFilterChanged()
    {
        CurrentPage = 1;
        await SearchDataAsync();
    }
}