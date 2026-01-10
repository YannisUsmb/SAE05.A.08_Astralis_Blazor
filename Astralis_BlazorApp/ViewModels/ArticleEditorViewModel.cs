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
        private readonly IArticleTypeService _articleTypeService;
        private readonly ITypeOfArticleService _typeOfArticleService;
        private readonly IUploadService _uploadService;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ArticleCreateDto article = new();
        [ObservableProperty] private List<ArticleTypeDto> articleTypes = new();
        [ObservableProperty] private List<int> selectedCategoryIds = new();

        [ObservableProperty] private bool isPublishing;
        [ObservableProperty] private bool showValidation;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private bool isUploadingCover;

        [ObservableProperty] private string typeSearchTerm = "";
        [ObservableProperty] private bool isTypeModalOpen;
        [ObservableProperty] private string newTypeName = "";
        [ObservableProperty] private string? typeCreationError;

        private int _tempIdCounter = -1;

        public IEnumerable<ArticleTypeDto> FilteredTypes => string.IsNullOrWhiteSpace(TypeSearchTerm)
            ? ArticleTypes
            : ArticleTypes.Where(t => t.Label.Contains(TypeSearchTerm, StringComparison.OrdinalIgnoreCase));

        public ArticleEditorViewModel(
            IArticleService articleService,
            IArticleTypeService articleTypeService,
            ITypeOfArticleService typeOfArticleService,
            IUploadService uploadService,
            NavigationManager navigation)
        {
            _articleService = articleService;
            _articleTypeService = articleTypeService;
            _typeOfArticleService = typeOfArticleService;
            _uploadService = uploadService;
            _navigation = navigation;
        }

        public async Task InitializeAsync()
        {
            Article = new ArticleCreateDto();
            SelectedCategoryIds.Clear();
            IsPublishing = false;
            ShowValidation = false;
            ErrorMessage = null;
            TypeSearchTerm = "";
            IsTypeModalOpen = false;
            _tempIdCounter = -1;

            try
            {
                ArticleTypes = await _articleTypeService.GetAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement types: {ex.Message}");
            }
        }

        public void OpenCreateTypeModal()
        {
            NewTypeName = TypeSearchTerm;
            TypeCreationError = null;
            IsTypeModalOpen = true;
        }

        public void CloseCreateTypeModal()
        {
            IsTypeModalOpen = false;
            NewTypeName = "";
        }

        public void ConfirmCreateTypeLocal()
        {
            if (string.IsNullOrWhiteSpace(NewTypeName))
            {
                TypeCreationError = "Le nom ne peut pas être vide.";
                return;
            }

            if (ArticleTypes.Any(t => t.Label.Equals(NewTypeName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                TypeCreationError = "Ce type existe déjà.";
                return;
            }

            var tempType = new ArticleTypeDto
            {
                Id = _tempIdCounter--,
                Label = NewTypeName.Trim(),
                Description = "Nouveau type (en attente de publication)"
            };

            ArticleTypes.Add(tempType);
            SelectedCategoryIds.Add(tempType.Id);

            TypeSearchTerm = "";
            CloseCreateTypeModal();
            OnPropertyChanged(nameof(ArticleTypes));
        }

        public void ToggleCategory(int categoryId)
        {
            if (SelectedCategoryIds.Contains(categoryId))
                SelectedCategoryIds.Remove(categoryId);
            else
                SelectedCategoryIds.Add(categoryId);

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
                if (!string.IsNullOrEmpty(url)) Article.CoverImageUrl = url;
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
                return await _uploadService.UploadImageAsync(file, UploadCategory.Articles);
            }
            catch { return null; }
        }

        public async Task SaveArticleAsync()
        {
            if (IsPublishing) return;
            IsPublishing = true;
            ErrorMessage = null;
            ShowValidation = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Article.Title) ||
                    string.IsNullOrWhiteSpace(Article.Content) ||
                    Article.Content == "<p><br></p>" ||
                    string.IsNullOrEmpty(Article.CoverImageUrl) ||
                    SelectedCategoryIds.Count == 0)
                {
                    ErrorMessage = "Veuillez remplir tous les champs obligatoires.";
                    IsPublishing = false;
                    return;
                }

                var finalCategoryIds = new List<int>();

                foreach (var id in SelectedCategoryIds)
                {
                    if (id < 0)
                    {
                        var tempType = ArticleTypes.First(t => t.Id == id);
                        var newTypeDto = new ArticleTypeDto { Label = tempType.Label };
                        var createdType = await _articleTypeService.AddAsync(newTypeDto);
                        finalCategoryIds.Add(createdType.Id);
                    }
                    else
                    {
                        finalCategoryIds.Add(id);
                    }
                }

                var createdArticle = await _articleService.AddAsync(Article);

                if (createdArticle != null && createdArticle.Id > 0)
                {
                    foreach (var typeId in finalCategoryIds)
                    {
                        var linkDto = new TypeOfArticleDto
                        {
                            ArticleId = createdArticle.Id,
                            ArticleTypeId = typeId
                        };
                        await _typeOfArticleService.AddAsync(linkDto);
                    }

                    _navigation.NavigateTo("/articles");
                }
                else
                {
                    throw new Exception("L'article n'a pas pu être créé (ID invalide).");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
                IsPublishing = false;
            }
        }
    }
    
}