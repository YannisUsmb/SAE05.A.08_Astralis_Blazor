using System.Collections.ObjectModel;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astralis_BlazorApp.ViewModels;

public partial class AdminDashboardViewModel : ObservableObject
{
    private readonly IAdminService _adminService;
    
    [ObservableProperty] private ObservableCollection<DiscoveryDto> pendingDiscoveries = new();
    
    [ObservableProperty] private bool isLoading;
    
    [ObservableProperty] private bool isRejectModalOpen;
    [ObservableProperty] private string rejectionReason = string.Empty;
    [ObservableProperty] private int? selectedDiscoveryId;

    public AdminDashboardViewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadDiscoveriesAsync();
    }

    private async Task LoadDiscoveriesAsync()
    {
        IsLoading = true;
        try
        {
            PendingDiscoveries.Clear();
            var result = await _adminService.GetPendingDiscoveriesAsync();
            foreach (var item in result)
            {
                PendingDiscoveries.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    public async Task ApproveDiscovery(int id)
    {
        var itemToRemove = PendingDiscoveries.FirstOrDefault(d => d.Id == id);
        try 
        {
            await _adminService.ApproveDiscoveryAsync(id);
            if (itemToRemove != null)
            {
                PendingDiscoveries.Remove(itemToRemove);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur validation : {ex.Message}");
            IsLoading = false;
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

        var itemToRemove = PendingDiscoveries.FirstOrDefault(d => d.Id == SelectedDiscoveryId);
        try
        {
            await _adminService.RejectDiscoveryAsync(SelectedDiscoveryId.Value, RejectionReason);
            IsRejectModalOpen = false;
            if (itemToRemove != null)
            {
                PendingDiscoveries.Remove(itemToRemove);
            }
            SelectedDiscoveryId = null;
            RejectionReason = "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur refus : {ex.Message}");
            IsLoading = false;
        }
    }
}