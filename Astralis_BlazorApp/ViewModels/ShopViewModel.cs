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
    [ObservableProperty] private string sortBy = "name";

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 30;
    [ObservableProperty] private bool hasNextPage = true;

    [ObservableProperty] private bool isLoading;

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
        IsLoading = true;
        try
        {
            var types = await _typeService.GetAllAsync();
            ProductCategories = new ObservableCollection<ProductCategoryDto>(types);
            await SearchDataAsync();
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
    public async Task NextPage() { if (HasNextPage) { CurrentPage++; await SearchDataAsync(); } }

    [RelayCommand]
    public async Task PreviousPage() { if (CurrentPage > 1) { CurrentPage--; await SearchDataAsync(); } }
}