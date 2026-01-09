using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ArticleListViewModel : ObservableObject
    {
        private readonly IArticleService _articleService;
        private readonly IArticleTypeService _typeService;
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;

        [ObservableProperty] private ObservableCollection<ArticleListDto> articles = new();
        [ObservableProperty] private ObservableCollection<ArticleTypeDto> articleTypes = new();
        [ObservableProperty] private ArticleFilterDto filter = new();

        [ObservableProperty] private int selectedTypeId = 0;
        [ObservableProperty] private bool isCommercialEditor;
        [ObservableProperty] private bool isLoading;

        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private int totalCount;

        public ArticleListViewModel(
            IArticleService articleService,
            IArticleTypeService typeService,
            NavigationManager navigation,
            AuthenticationStateProvider authStateProvider)
        {
            _articleService = articleService;
            _typeService = typeService;
            _navigation = navigation;
            _authStateProvider = authStateProvider;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                IsCommercialEditor = user.IsInRole("Rédacteur Commercial") || user.IsInRole("Admin");

                var types = await _typeService.GetAllAsync();
                ArticleTypes = new ObservableCollection<ArticleTypeDto>(types);

                await SearchDataAsync();
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
                Filter.TypeIds = SelectedTypeId != 0 ? new List<int> { SelectedTypeId } : null;

                var result = await _articleService.SearchAsync(Filter);

                Articles = new ObservableCollection<ArticleListDto>(result.Items);

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur recherche: {ex.Message}");
                Articles.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateUrl()
        {
            var uri = _navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["search"] = string.IsNullOrWhiteSpace(Filter.SearchTerm) ? null : Filter.SearchTerm,
                ["type"] = SelectedTypeId == 0 ? null : SelectedTypeId,
                ["premium"] = Filter.IsPremium == true ? true : null,
                ["sort"] = Filter.SortBy == "date_desc" ? null : Filter.SortBy,
                ["page"] = Filter.PageNumber <= 1 ? null : Filter.PageNumber
            });

            _navigation.NavigateTo(uri);
        }

        public void NavigateToCreate()
        {
            _navigation.NavigateTo("/admin/articles/nouveau");
        }

        public void NavigateToDetails(int id)
        {
            _navigation.NavigateTo($"/articles/{id}");
        }
    }
}