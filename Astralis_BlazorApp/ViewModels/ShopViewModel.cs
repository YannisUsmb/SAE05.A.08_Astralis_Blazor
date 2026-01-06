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
    private readonly IProductService _productService;
    private readonly IProductCategoryService _typeService;
    private readonly ICartService _cartService;

    private CancellationTokenSource? _searchCts;

    [ObservableProperty] private ObservableCollection<ProductListDto> products = new();
    [ObservableProperty] private ObservableCollection<ProductCategoryDto> productCategories = new();

    [ObservableProperty] private ProductFilterDto filter = new();
    [ObservableProperty] private int selectedTypeId = 0;
    [ObservableProperty] private string sortBy = "name";

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool is3DVisible;
    [ObservableProperty] private ProductListDto? selectedProduct;
    [ObservableProperty] private ProductDetailDto? selectedProductDetails;

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

    public ShopViewModel(
        IProductService productService,
        IProductCategoryService typeService,
        ICartService cartService)
    {
        _productService = productService;
        _typeService = typeService;
        _cartService = cartService;
    }

    private async void TriggerDebounceSearch(string text)
    {
        Filter.SearchText = text;
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(500, token);
            if (!token.IsCancellationRequested)
            {
                await ApplyFilterAsync();
            }
        }
        catch (TaskCanceledException) { }
    }


    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var types = await _typeService.GetAllAsync();
            ProductCategories = new ObservableCollection<ProductCategoryDto>(types);

            await SearchDataAsync();

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
            SelectedProductDetails = null;
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
            Console.WriteLine($"Erreur d�tails : {ex.Message}");
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
    [RelayCommand]
    public async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await SearchDataAsync();
        }
    }

    [RelayCommand]
    public async Task AddToCart(ProductListDto product)
    {
        if (product == null) return;

        try
        {
            await _cartService.AddToCartAsync(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur ajout panier : {ex.Message}");
        }
    }
}