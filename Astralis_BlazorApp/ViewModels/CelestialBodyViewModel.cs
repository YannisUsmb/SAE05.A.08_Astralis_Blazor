using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Extensions;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels;

public partial class CelestialBodyViewModel : ObservableObject
{
    private readonly ICelestialBodyService _bodyService;
    private readonly ICelestialBodyTypeService _typeService;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IDiscoveryService _discoveryService;

    [ObservableProperty] private ObservableCollection<CelestialBodyListDto> celestialBodies = new();
    [ObservableProperty] private ObservableCollection<CelestialBodyTypeDto> celestialBodyTypes = new();
    [ObservableProperty] private ObservableCollection<CelestialBodySubtypeDto> celestialSubtypes = new();
    
    [ObservableProperty] private bool isAuthenticated;
    [ObservableProperty] private CelestialBodyFilterDto filter = new();

    [ObservableProperty] private int selectedTypeId = 0;
    [ObservableProperty] private int selectedSubtypeId = 0;
    [ObservableProperty] private string sortBy = "name_asc";

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private CelestialBodyListDto? selectedBody;
    [ObservableProperty] private CelestialBodyDetailDto? selectedBodyDetails;
    
    // --- GESTION DES ALIAS & MODALES ---
    [ObservableProperty] private bool isAliasModalOpen;
    [ObservableProperty] private string aliasInputValue = string.Empty;
    [ObservableProperty] private bool isSubmittingAlias;

    // Nouvelles modales pour remplacer les alert() moches
    [ObservableProperty] private bool isPaymentModalOpen;
    [ObservableProperty] private bool isSuccessModalOpen;
    [ObservableProperty] private bool isDeleteConfirmModalOpen;

    public CelestialBodyViewModel(
        ICelestialBodyService bodyService, 
        ICelestialBodyTypeService typeService, 
        NavigationManager navigationManager, 
        AuthenticationStateProvider authStateProvider, 
        IDiscoveryService discoveryService)
    {
        _bodyService = bodyService;
        _typeService = typeService;
        _navigationManager = navigationManager;
        _authStateProvider = authStateProvider;
        _discoveryService = discoveryService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
            
            var types = await _typeService.GetAllAsync();
            CelestialBodyTypes = new ObservableCollection<CelestialBodyTypeDto>(types);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur init : {ex.Message}");
        }
        finally { IsLoading = false; }
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
        finally { IsLoading = false; }
    }

    public async Task OnTypeChanged()
    {
        // Reset filters based on type... (code inchangé pour abréger)
        Filter.PlanetFilter = null; Filter.StarFilter = null; Filter.AsteroidFilter = null;
        Filter.GalaxyFilter = null; Filter.CometFilter = null; Filter.SatelliteFilter = null;

        switch (SelectedTypeId)
        {
            case 1: Filter.StarFilter = new StarFilterDto(); break;
            case 2: Filter.PlanetFilter = new PlanetFilterDto(); break;
            case 3: Filter.AsteroidFilter = new AsteroidFilterDto(); break;
            case 4: Filter.CometFilter = new CometFilterDto(); break;
            case 5: Filter.GalaxyFilter = new GalaxyQuasarFilterDto(); break;
            case 6: Filter.SatelliteFilter = new SatelliteFilterDto(); break;
        }

        SelectedSubtypeId = 0;
        Filter.SubtypeId = null;

        if (SelectedTypeId == 0) CelestialSubtypes.Clear();
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
        if (SortBy.EndsWith("_desc")) { Filter.SortBy = SortBy.Replace("_desc", ""); Filter.SortAscending = false; }
        else if (SortBy.EndsWith("_asc")) { Filter.SortBy = SortBy.Replace("_asc", ""); Filter.SortAscending = true; }
        else { Filter.SortBy = SortBy; Filter.SortAscending = true; }
        await OnFilterChanged();
    }
    
    [RelayCommand]
    public void NavigateToDiscoveryForm()
    {
        if (!IsAuthenticated) { _navigationManager.NavigateToLogin("/decouverte/nouvelle"); return; }
        _navigationManager.NavigateTo("/decouverte/nouvelle");
    }

    public async Task OnFilterChanged()
    {
        CurrentPage = 1;
        await SearchDataAsync();
    }
    
    [RelayCommand]
    public async Task ShowDetails(CelestialBodyListDto body)
    {
        SelectedBody = body;
        IsLoading = true;
        Is3DVisible = true;
        string url = BuildUrlWithFilters(body.Id);
        _navigationManager.NavigateTo(url, forceLoad: false);
        try { SelectedBodyDetails = await _bodyService.GetDetailsByIdAsync(body.Id); }
        catch { SelectedBodyDetails = null; }
        finally { IsLoading = false; }
    }

    public async Task ShowDetailsById(int id)
    {
        IsLoading = true;
        Is3DVisible = true;
        try 
        { 
            SelectedBodyDetails = await _bodyService.GetDetailsByIdAsync(id);
            SelectedBody = new CelestialBodyListDto { Id = id, Name = SelectedBodyDetails?.Name };
        }
        catch { SelectedBodyDetails = null; }
        finally { IsLoading = false; }
    }

    private string BuildUrlWithFilters(int? bodyId = null)
    {
        var queryParams = new List<string>();
        if (bodyId.HasValue) queryParams.Add($"{bodyId.Value}");
        if (!string.IsNullOrEmpty(Filter.SearchText)) queryParams.Add($"search={Uri.EscapeDataString(Filter.SearchText)}");
        if (CurrentPage > 1) queryParams.Add($"page={CurrentPage}");
        string baseUrl = "/corps-celestes";
        if (queryParams.Any()) baseUrl += "?" + string.Join("&", queryParams);
        return baseUrl;
    }

    [RelayCommand]
    public void BackToList()
    {
        Is3DVisible = false;
        SelectedBody = null;
        SelectedBodyDetails = null;
        string url = BuildUrlWithFilters();
        _navigationManager.NavigateTo(url, forceLoad: false);
    }

    [RelayCommand]
    public async Task NextPage() { if (HasNextPage) { CurrentPage++; await SearchDataAsync(); } }

    [RelayCommand]
    public async Task PreviousPage() { if (CurrentPage > 1) { CurrentPage--; await SearchDataAsync(); } }
    
    // --- GESTION DES ALIAS ---

    [RelayCommand]
    public void OpenAliasModal()
    {
        AliasInputValue = SelectedBodyDetails?.Alias ?? "";
        IsAliasModalOpen = true;
    }

    [RelayCommand]
    public void CloseModals()
    {
        IsAliasModalOpen = false;
        IsPaymentModalOpen = false;
        IsSuccessModalOpen = false;
        IsDeleteConfirmModalOpen = false;
        AliasInputValue = "";
    }

    [RelayCommand]
    public async Task SubmitAliasAsync()
    {
        if (string.IsNullOrWhiteSpace(AliasInputValue) || SelectedBodyDetails?.Discovery == null) return;

        IsSubmittingAlias = true;
        try
        {
            var dto = new DiscoveryAliasDto { Alias = AliasInputValue };
            
            await _discoveryService.ProposeAliasAsync(SelectedBodyDetails.Discovery.Id, dto);

            await ShowDetailsById(SelectedBodyDetails.Id);
            CloseModals();
            IsSuccessModalOpen = true; 
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            IsAliasModalOpen = false;
            IsPaymentModalOpen = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
        finally
        {
            IsSubmittingAlias = false;
        }
    }
    
    [RelayCommand]
    public async Task ProceedToPayment()
    {
        if (SelectedBodyDetails?.Discovery == null) return;

        try 
        {
            string stripeUrl = await _discoveryService.CreateAliasPaymentSessionAsync(
                SelectedBodyDetails.Discovery.Id, 
                AliasInputValue
            );

            if (!string.IsNullOrEmpty(stripeUrl))
            {
                _navigationManager.NavigateTo(stripeUrl, forceLoad: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur init paiement : {ex.Message}");
        }
    }

    public async Task HandlePaymentCallbackAsync(string sessionId, int discoveryId)
    {
        IsLoading = true;
        try
        {
            await _discoveryService.ValidateAliasPaymentAsync(sessionId);
 
            await ShowDetailsById(discoveryId);

            IsSuccessModalOpen = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur validation paiement : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    public void RequestDeleteAlias()
    {
        IsDeleteConfirmModalOpen = true;
    }

    [RelayCommand]
    public async Task ConfirmDeleteAlias()
    {
        if (SelectedBodyDetails?.Discovery == null) return;

        try
        {
            await _discoveryService.RemoveAliasAsync(SelectedBodyDetails.Discovery.Id);
            await ShowDetailsById(SelectedBodyDetails.Id);
            CloseModals();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur suppression alias : {ex.Message}");
        }
    }
    
    [RelayCommand]
    public void NavigateToPremium()
    {
        CloseModals();
        _navigationManager.NavigateTo("/premium");
    }
}