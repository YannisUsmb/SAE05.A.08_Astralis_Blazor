using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ShopViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _typeService;
        private readonly ICartService _cartService;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        private CancellationTokenSource? _searchCts;

        [ObservableProperty] private ObservableCollection<ProductListDto> products = new();
        [ObservableProperty] private ObservableCollection<ProductCategoryDto> productCategories = new();

        [ObservableProperty] private ProductFilterDto filter = new();
        [ObservableProperty] private bool showDeleteConfirmation;
        [ObservableProperty] private ProductListDto? productToDelete;
        [ObservableProperty] private int selectedTypeId = 0;
        [ObservableProperty] private string sortBy = "name";
        [ObservableProperty] private decimal? minPrice;
        [ObservableProperty] private decimal? maxPrice;

        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int pageSize = 30;
        [ObservableProperty] private bool hasNextPage = true;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isCommercialEditor;

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
            ICartService cartService,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider)
        {
            _productService = productService;
            _typeService = typeService;
            _cartService = cartService;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
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
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                IsCommercialEditor = user.IsInRole("Rédacteur Commercial") || user.IsInRole("Admin");

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
                Filter.MinPrice = MinPrice;
                Filter.MaxPrice = MaxPrice;
                Filter.PageNumber = CurrentPage;
                Filter.PageSize = PageSize;

                var results = await _productService.SearchAsync(Filter);

                HasNextPage = results.Count() == PageSize;

                IEnumerable<ProductListDto> sortedList = results;
                switch (SortBy)
                {
                    case "category": sortedList = results.OrderBy(p => p.CategoryLabel).ThenBy(p => p.Label); break;
                    case "price_asc": sortedList = results.OrderBy(p => p.Price); break;
                    case "price_desc": sortedList = results.OrderByDescending(p => p.Price); break;
                    case "name": default: sortedList = results.OrderBy(p => p.Label); break;
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
            finally { IsLoading = false; }
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
            if (HasNextPage) { CurrentPage++; await SearchDataAsync(); }
        }

        [RelayCommand]
        public async Task PreviousPage()
        {
            if (CurrentPage > 1) { CurrentPage--; await SearchDataAsync(); }
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
        public void RequestDelete(ProductListDto product)
        {
            ProductToDelete = product;
            ShowDeleteConfirmation = true;
        }

        public void CancelDelete()
        {
            ShowDeleteConfirmation = false;
            ProductToDelete = null;
        }

        public async Task ConfirmDeleteAsync()
        {
            if (ProductToDelete == null) return;

            try
            {
                await _productService.DeleteAsync(ProductToDelete.Id);

                var productInList = Products.FirstOrDefault(p => p.Id == ProductToDelete.Id);
                if (productInList != null)
                {
                    Products.Remove(productInList);
                }

                if (SelectedProductDetails?.Id == ProductToDelete.Id)
                {
                    SelectedProductDetails = null;
                }

                ShowDeleteConfirmation = false;
                ProductToDelete = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression : {ex.Message}");
            }
        }
        public void NavigateToCreate()
        {
            _navigation.NavigateTo("/admin/produits/creer");
        }
    }
}