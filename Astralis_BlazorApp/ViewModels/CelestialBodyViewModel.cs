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
    
    [ObservableProperty] private ObservableCollection<CelestialBodyListDto> celestialBodies = new();
    [ObservableProperty] private ObservableCollection<CelestialBodyTypeDto> celestialBodyTypes = new();
    [ObservableProperty] private ObservableCollection<CelestialBodySubtypeDto> celestialSubtypes = new();
    
    [ObservableProperty] private CelestialBodyFilterDto filter = new();
    
    [ObservableProperty] private int selectedTypeId = 0;
    [ObservableProperty] private int selectedSubtypeId = 0;
    
    [ObservableProperty] private string sortBy = "name";

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private CelestialBodyListDto? selectedBody;
    
    public CelestialBodyViewModel(ICelestialBodyService bodyService, ICelestialBodyTypeService typeService)
    {
        _bodyService = bodyService;
        _typeService = typeService;
    }
    
    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var types = await _typeService.GetAllAsync();
            CelestialBodyTypes = new ObservableCollection<CelestialBodyTypeDto>(types);
            
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
    
    [RelayCommand]
    public async Task SearchDataAsync()
    {
        IsLoading = true;
        try
        {
            Filter.CelestialBodyTypeIds = SelectedTypeId != 0 ? new List<int> { SelectedTypeId } : null;

            var results = await _bodyService.SearchAsync(Filter, CurrentPage, PageSize);
            
            HasNextPage = results.Count == PageSize;

            CelestialBodies = new ObservableCollection<CelestialBodyListDto>(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur recherche : {ex.Message}");
            CelestialBodies.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    
    public async Task OnTypeChanged()
    {
        Filter.PlanetFilter = null;
        Filter.StarFilter = null;
        Filter.AsteroidFilter = null;
        Filter.GalaxyFilter = null;
        Filter.CometFilter = null;
        Filter.SatelliteFilter = null;

        switch (SelectedTypeId)
        {
            case 1: Filter.StarFilter = new StarFilterDto(); break;
            case 2: Filter.PlanetFilter = new PlanetFilterDto(); break;
            case 3: Filter.AsteroidFilter = new AsteroidFilterDto(); break;
            case 4: Filter.SatelliteFilter = new SatelliteFilterDto(); break;
            case 5: Filter.GalaxyFilter = new GalaxyQuasarFilterDto(); break;
            case 6: Filter.CometFilter = new CometFilterDto(); break;
            
        }

        SelectedSubtypeId = 0;
        Filter.SubtypeId = null;
        
        if (SelectedTypeId == 0)
        {
            CelestialSubtypes.Clear();
        }
        else
        {
            var result = await _bodyService.GetSubtypesAsync(SelectedTypeId);
            CelestialSubtypes = new ObservableCollection<CelestialBodySubtypeDto>(result);
        }

        await OnFilterChanged();
    }
    
    public async Task OnSubtypeChanged()
    {
        Filter.SubtypeId = SelectedSubtypeId == 0 ? null : SelectedSubtypeId;
        await OnFilterChanged();
    }
    
    public async Task OnSortChanged()
    {
        Filter.SortBy = SortBy;
        Filter.SortAscending = true;
        
        await OnFilterChanged();
    }
    
    public async Task OnFilterChanged()
    {
        CurrentPage = 1;
        await SearchDataAsync();
    }
    
    [RelayCommand]
    public void ShowDetails(CelestialBodyListDto body)
    {
        SelectedBody = body;
        Is3DVisible = true;
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
}
