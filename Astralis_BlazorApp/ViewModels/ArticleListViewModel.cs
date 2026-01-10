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

        [ObservableProperty] private bool isLoading = true;

        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private int totalCount;

        private System.Timers.Timer _debounceTimer;

        private bool _isBusy = false;

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

            _debounceTimer = new System.Timers.Timer(500);
            _debounceTimer.Elapsed += async (s, e) =>
            {
                await InvokeAsync(() => SearchDataAsync(updateUrl: false));
            };
            _debounceTimer.AutoReset = false;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                IsCommercialEditor = user.IsInRole("Rédacteur Commercial") || user.IsInRole("Admin");

                var types = await _typeService.GetAllAsync();
                var sortedTypes = types
                    .OrderBy(t => t.Label.Equals("Autre", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(t => t.Label);

                ArticleTypes = new ObservableCollection<ArticleTypeDto>(sortedTypes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [RelayCommand]
        public async Task SearchDataAsync(bool updateUrl = true)
        {
            if (_isBusy) return;
            _isBusy = true;

            IsLoading = true;

            try
            {
                Filter.TypeIds = SelectedTypeId != 0 ? new List<int> { SelectedTypeId } : null;

                var result = await _articleService.SearchAsync(Filter);

                Articles = new ObservableCollection<ArticleListDto>(result.Items);
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;

                if (updateUrl) UpdateUrl(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
                _isBusy = false;
            }
        }

        public void UpdateUrl(bool forceLoad = false)
        {
            var uri = _navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["search"] = string.IsNullOrWhiteSpace(Filter.SearchTerm) ? null : Filter.SearchTerm,
                ["type"] = SelectedTypeId == 0 ? null : SelectedTypeId,
                ["premium"] = Filter.IsPremium == true ? true : null,
                ["sort"] = Filter.SortBy == "date_desc" ? null : Filter.SortBy,
                ["page"] = Filter.PageNumber <= 1 ? null : Filter.PageNumber
            });

            _navigation.NavigateTo(uri, forceLoad);
        }

        public void OnSearchInput(ChangeEventArgs e)
        {
            Filter.SearchTerm = e.Value?.ToString();
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void NavigateToCreate()
        {
            _navigation.NavigateTo("/admin/articles/nouveau");
        }

        public void NavigateToDetails(int id)
        {
            _navigation.NavigateTo($"/articles/{id}");
        }

        public async Task DeleteArticleAsync(int articleId)
        {
            try
            {
                await _articleService.DeleteAsync(articleId);

                var articleToRemove = Articles.FirstOrDefault(a => a.Id == articleId);
                if (articleToRemove != null)
                {
                    Articles.Remove(articleToRemove);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression: {ex.Message}");
            }
        }

        private async Task InvokeAsync(Func<Task> action) => await action();
    }
}