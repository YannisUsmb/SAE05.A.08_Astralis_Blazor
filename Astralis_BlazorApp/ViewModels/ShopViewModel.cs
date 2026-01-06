using System.Collections.ObjectModel;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels;

public partial class ShopViewModel : ObservableObject
{
    // --- Services injectés ---
    private readonly IProductService _productService;
    private readonly IProductCategoryService _typeService;
    private readonly ICartService _cartService;
    
    // --- Gestion du Debounce (Recherche) ---
    private CancellationTokenSource? _searchCts;

    // --- Collections de données ---
    [ObservableProperty] private ObservableCollection<ProductListDto> products = new();
    [ObservableProperty] private ObservableCollection<ProductCategoryDto> productCategories = new();
    
    // --- Filtres et Tri ---
    [ObservableProperty] private ProductFilterDto filter = new();
    
    [ObservableProperty] private int selectedTypeId = 0;
    [ObservableProperty] private int selectedSubtypeId = 0;
    
    [ObservableProperty] private string sortBy = "name";

    // --- Pagination ---
    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    // --- États de l'interface ---
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private ProductListDto? selectedProduct;
    [ObservableProperty] private ProductDetailDto? selectedProductDetails;
    
    // --- PROPRIÉTÉ DE RECHERCHE (Implémentation Manuelle pour Debounce) ---
    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                TriggerDebounceSearch(value);
            }
        }
    }
    
    // --- CONSTRUCTEUR ---
    public ShopViewModel(
        IProductService productService,
        IProductCategoryService typeService,
        ICartService cartService)
    {
        _productService = productService;
        _typeService = typeService;
        _cartService = cartService;
    }
    
    // --- LOGIQUE DE RECHERCHE ---

    private async void TriggerDebounceSearch(string text)
    {
        Filter.SearchText = text;
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(500, token); // Attente de 500ms
            if (!token.IsCancellationRequested)
            {
                await ApplyFilterAsync();
            }
        }
        catch (TaskCanceledException) { /* Ignorer */ }
    }

    // --- COMMANDES (Actions) ---

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            // 1. Chargement des catégories
            var types = await _typeService.GetAllAsync();
            ProductCategories = new ObservableCollection<ProductCategoryDto>(types);
            
            // 2. Chargement initial des produits
            await SearchDataAsync();

            // 3. Chargement de l'état du panier (pour le badge)
            await _cartService.LoadCartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur init : {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ApplyFilterAsync()
        {
        CurrentPage = 1;
        await SearchDataAsync();
        
    }
    
    [RelayCommand]
    public async Task SearchDataAsync()
    {
        IsLoading = true;
        try
        {
            Filter.ProductCategoryIds = SelectedTypeId != 0 ? new List<int> { SelectedTypeId } : null;
            Filter.PageNumber = CurrentPage;
            Filter.PageSize = PageSize;

            var results = await _productService.SearchAsync(Filter);

            HasNextPage = results.Count == PageSize;
            
            // Tri côté client (si l'API ne le gère pas déjà)
            IEnumerable<ProductListDto> sortedList = results;
            switch (SortBy)
            {
                case "category":
                    sortedList = results.OrderBy(p => p.CategoryLabel).ThenBy(p => p.Label);
                    break;
                case "price_asc":
                    sortedList = results.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    sortedList = results.OrderByDescending(p => p.Price);
                    break;
                case "name":
                default:
                    sortedList = results.OrderBy(p => p.Label);
                    break;
            }

            Products = new ObservableCollection<ProductListDto>(sortedList);
            SelectedProductDetails = null; // Reset détail si on cherche
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur recherche : {ex.Message}");
            Products.Clear();
        }
        finally
        {
            IsLoading = false;
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
    public async Task ShowDetails(ProductListDto body)
    {
        if (body == null) return;

        IsLoading = true;
        SelectedProduct = body;
    
        try
        {
            var details = await _productService.GetByIdAsync(body.Id);
            SelectedProductDetails = details;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur détails : {ex.Message}");
            SelectedProductDetails = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void BackToList()
    {
        SelectedProductDetails = null;
        SelectedProduct = null;
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
    // --- CORRECTION MAJEURE ICI : ASYNC ---
    [RelayCommand]
    public async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await SearchDataAsync();
        }
    }

    // --- CORRECTION MAJEURE ICI : ASYNC ---
    [RelayCommand]
    public async Task AddToCart(ProductListDto product)
    {
        if (product == null) return;

        try
        {
            // Appel asynchrone au service API
            await _cartService.AddToCartAsync(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur ajout panier : {ex.Message}");
        }
    }
}
