using System.Collections.ObjectModel;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astralis_BlazorApp.ViewModels;

public partial class ShopViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _typeService;
    
    [ObservableProperty] private ObservableCollection<ProductListDto> products = new();
    [ObservableProperty] private ObservableCollection<ProductCategoryDto> productCategories = new();
    
    [ObservableProperty] private ProductFilterDto filter = new();
    
    [ObservableProperty] private int selectedTypeId = 0;
    [ObservableProperty] private int selectedSubtypeId = 0;
    
    [ObservableProperty] private string sortBy = "name";

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private ProductListDto? selectedProduct;
    
    [ObservableProperty] private ProductDetailDto? selectedProductDetails;
    
    public ShopViewModel(IProductService bodyService, IProductCategoryService typeService)
    {
        _productService = bodyService;
        _typeService = typeService;
    }
    
    [RelayCommand]
    public async Task InitializeAsync()
    {
        isLoading = true;
        try
        {
            var types = await _typeService.GetAllAsync();
            productCategories = new ObservableCollection<ProductCategoryDto>(types);
            
            await SearchDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur init : {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }
    
    [RelayCommand]
    public async Task SearchDataAsync()
    {
        isLoading = true;
        try
        {
            filter.ProductCategoryIds = selectedTypeId != 0 ? new List<int> { selectedTypeId } : null;

            var results = await _productService.SearchAsync(filter);
            
            hasNextPage = results.Count == pageSize;

            products = new ObservableCollection<ProductListDto>(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur recherche : {ex.Message}");
            products.Clear();
        }
        finally
        {
            isLoading = false;
        }
    }

    public async Task OnTypeChanged()
    {        
    }
    
    public async Task OnSubtypeChanged()
    {
    }
    
    public async Task OnSortChanged()
    {
    }
    
    public async Task OnFilterChanged()
    {
    }
    
    [RelayCommand]
    public async void ShowDetails(ProductListDto body)
    {
        selectedProduct = body;
        isLoading = true;
    
        try
        {
            Console.WriteLine($"[DEBUG] Fetching details for body ID: {body.Id}");
            selectedProductDetails = await _productService.GetByIdAsync(body.Id);
            Console.WriteLine($"[DEBUG] Details loaded: {selectedProductDetails != null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error loading details: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            selectedProductDetails = null;
        }
        finally
        {
            Console.WriteLine($"[DEBUG] Setting isLoading to false");
            isLoading = false;
        }
    }

    [RelayCommand]
    public void BackToList()
    {
        selectedProduct = null;
        selectedProductDetails = null;
    }
    
    [RelayCommand]
    public async Task NextPage()
    {
        if (hasNextPage)
        {
            currentPage++;
            await SearchDataAsync();
        }
    }

    [RelayCommand]
    public async Task PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            await SearchDataAsync();
        }
    }
}
