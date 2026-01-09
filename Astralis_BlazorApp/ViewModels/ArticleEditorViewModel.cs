using Astralis.Shared.DTOs;
using Astralis.Shared.Enums;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ArticleEditorViewModel : ObservableObject
    {
        private readonly IArticleService _articleService;
        private readonly IArticleTypeService _typeService;
        private readonly IUploadService _uploadService;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ArticleCreateDto article = new();
        [ObservableProperty] private List<ArticleTypeDto> articleTypes = new();

        [ObservableProperty] private List<int> selectedCategoryIds = new();

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isUploadingCover;
        [ObservableProperty] private bool showValidation;
        [ObservableProperty] private string? errorMessage;

        public ArticleEditorViewModel(
            IArticleService articleService,
            IArticleTypeService typeService,
            IUploadService uploadService,
            NavigationManager navigation)
        {
            _articleService = articleService;
            _typeService = typeService;
            _uploadService = uploadService;
            _navigation = navigation;
        }

        public async Task InitializeAsync()
        {
            ArticleTypes = await _typeService.GetAllAsync();
        }

        public void ToggleCategory(int categoryId)
        {
            if (SelectedCategoryIds.Contains(categoryId))
            {
                SelectedCategoryIds.Remove(categoryId);
            }
            else
            {
                SelectedCategoryIds.Add(categoryId);
            }
            OnPropertyChanged(nameof(SelectedCategoryIds));
        }

        public async Task UploadCoverImageAsync(InputFileChangeEventArgs e)
        {
            IsUploadingCover = true;
            ErrorMessage = null;
            try
            {
                if (e.File.Size > 5 * 1024 * 1024)
                {
                    ErrorMessage = "L'image de couverture est trop lourde (Max 5Mo).";
                    return;
                }

                string url = await _uploadService.UploadImageAsync(e.File, UploadCategory.Articles);

                if (!string.IsNullOrEmpty(url))
                {
                    Article.CoverImageUrl = url;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur upload couverture : {ex.Message}";
            }
            finally
            {
                IsUploadingCover = false;
            }
        }

        public async Task<string?> UploadImageToContentAsync(IBrowserFile file)
        {
            try
            {
                if (file.Size > 10 * 1024 * 1024) return null;

                string url = await _uploadService.UploadImageAsync(file, UploadCategory.Articles);
                return url;
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveArticleAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Article.CategoryIds = SelectedCategoryIds;

                if (string.IsNullOrWhiteSpace(Article.Title))
                {
                    ErrorMessage = "Le titre est obligatoire.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Article.Content))
                {
                    ErrorMessage = "Le contenu de l'article est vide.";
                    ShowValidation = true;
                    return;
                }

                if (SelectedCategoryIds.Count == 0)
                {
                    ErrorMessage = "Veuillez sélectionner au moins une catégorie.";
                    return;
                }

                await _articleService.AddAsync(Article);
                _navigation.NavigateTo("/articles");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la publication : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}