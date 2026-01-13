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
            var result = await _adminService.GetPendingDiscoveriesAsync();
            PendingDiscoveries = new ObservableCollection<DiscoveryDto>(result);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    public async Task ApproveDiscovery(int id)
    {
        IsLoading = true;
        try 
        {
            await _adminService.ApproveDiscoveryAsync(id);
            await LoadDiscoveriesAsync(); 
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

        IsLoading = true;
        try
        {
            await _adminService.RejectDiscoveryAsync(SelectedDiscoveryId.Value, RejectionReason);
            IsRejectModalOpen = false;
            await LoadDiscoveriesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur refus : {ex.Message}");
            IsLoading = false;
        }
    }
}