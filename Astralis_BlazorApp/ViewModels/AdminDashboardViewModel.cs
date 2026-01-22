using System.Collections.ObjectModel;
using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astralis_BlazorApp.ViewModels;

public partial class AdminDashboardViewModel : ObservableObject
{
    private readonly IDiscoveryService _discoveryService;
    
    // --- PROPERTIES ---
    [ObservableProperty] private ObservableCollection<DiscoveryDto> pendingDiscoveries = new();
    [ObservableProperty] private ObservableCollection<DiscoveryDto> pendingAliases = new();
    
    [ObservableProperty] private bool isLoading;
    
    // --- REJECTION MODAL ---
    [ObservableProperty] private bool isRejectModalOpen;
    [ObservableProperty] private string rejectionReason = string.Empty;
    [ObservableProperty] private int? selectedDiscoveryId;

    public AdminDashboardViewModel(IDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            PendingDiscoveries.Clear();
            PendingAliases.Clear();

            // 1. Charger les découvertes (Status 2)
            var discoveryFilter = new DiscoveryFilterDto { DiscoveryStatusId = 2 };
            var discoveries = await _discoveryService.SearchAsync(discoveryFilter);
            foreach (var item in discoveries) PendingDiscoveries.Add(item);

            // 2. Charger les alias (Status 1)
            var aliasFilter = new DiscoveryFilterDto { AliasStatusId = 1 };
            var aliases = await _discoveryService.SearchAsync(aliasFilter);
            foreach (var item in aliases) PendingAliases.Add(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement dashboard : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- DISCOVERIES MANAGEMENT ---

    [RelayCommand]
    public async Task ApproveDiscovery(int id)
    {
        try 
        {
            var dto = new DiscoveryModerationDto { DiscoveryStatusId = 3 };
            await _discoveryService.ModerateStatusAsync(id, dto);
            
            var item = PendingDiscoveries.FirstOrDefault(d => d.Id == id);
            if (item != null) PendingDiscoveries.Remove(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur validation découverte : {ex.Message}");
        }
    }
    
    [RelayCommand]
    public void OpenRejectModal(int id)
    {
        SelectedDiscoveryId = id;
        RejectionReason = "";
        IsRejectModalOpen = true;
    }

    [RelayCommand]
    public void CancelRejection()
    {
        IsRejectModalOpen = false;
        SelectedDiscoveryId = null;
    }

    [RelayCommand]
    public async Task ConfirmRejection()
    {
        if (SelectedDiscoveryId == null || string.IsNullOrWhiteSpace(RejectionReason)) return;

        try
        {
            var dto = new DiscoveryModerationDto { DiscoveryStatusId = 4 }; 
            await _discoveryService.ModerateStatusAsync(SelectedDiscoveryId.Value, dto);

            var item = PendingDiscoveries.FirstOrDefault(d => d.Id == SelectedDiscoveryId);
            if (item != null) PendingDiscoveries.Remove(item);

            IsRejectModalOpen = false;
            SelectedDiscoveryId = null;
            RejectionReason = "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur refus découverte : {ex.Message}");
        }
    }

    // --- ALIAS MANAGEMENT ---

    [RelayCommand]
    public async Task ApproveAlias(int id)
    {
        try
        {
            var dto = new DiscoveryModerationDto { AliasStatusId = 2 };
            var success = await _discoveryService.ModerateAliasAsync(id, dto);

            if (success)
            {
                var item = PendingAliases.FirstOrDefault(a => a.Id == id);
                if (item != null) PendingAliases.Remove(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur validation alias : {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task RejectAlias(int id)
    {
        try
        {
            var dto = new DiscoveryModerationDto { AliasStatusId = 3 };
            var success = await _discoveryService.ModerateAliasAsync(id, dto);

            if (success)
            {
                var item = PendingAliases.FirstOrDefault(a => a.Id == id);
                if (item != null) PendingAliases.Remove(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur refus alias : {ex.Message}");
        }
    }
}