using Astralis.Shared.DTOs;
using Astralis.Shared.Enums; // Assure-toi que cet enum existe ou remplace par string
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ProductEditorViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _categoryService;
        private readonly IUploadService _uploadService;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ProductCreateDto product = new();
        [ObservableProperty] private List<ProductCategoryDto> categories = new();

        [ObservableProperty] private bool isSaving;
        [ObservableProperty] private bool isUploadingImage;
        [ObservableProperty] private string? errorMessage;

        // Stocke l'ID s'il existe
        [ObservableProperty] private int? productId;

        public bool IsEditMode => productId.HasValue && productId.Value > 0;

        public ProductEditorViewModel(
            IProductService productService,
            IProductCategoryService categoryService,
            IUploadService uploadService,
            NavigationManager navigation)
        {
            _productService = productService;
            _categoryService = categoryService;
            _uploadService = uploadService;
            _navigation = navigation;
        }

        public async Task InitializeAsync(int? id = null)
        {
            Product = new ProductCreateDto();
            IsSaving = false;
            ErrorMessage = null;
            ProductId = id;

            try
            {
                // Chargement des catégories pour la liste déroulante
                var cats = await _categoryService.GetAllAsync();
                Categories = cats.ToList();

                // Si mode édition, on charge le produit existant
                if (IsEditMode)
                {
                    await LoadExistingProductAsync(ProductId.Value);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur d'initialisation : {ex.Message}";
            }
        }

        private async Task LoadExistingProductAsync(int id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null)
            {
                ErrorMessage = "Produit introuvable.";
                // Délai pour laisser l'utilisateur lire le message si besoin, ou redirection directe
                NavigateBack();
                return;
            }

            // Mapping vers le DTO de création/édition
            Product = new ProductCreateDto
            {
                Label = existing.Label,
                Description = existing.Description,
                Price = existing.Price,
                ProductPictureUrl = existing.ProductPictureUrl,
                ProductCategoryId = existing.CategoryId
            };
        }

        public async Task UploadProductImageAsync(InputFileChangeEventArgs e)
        {
            IsUploadingImage = true;
            ErrorMessage = null;
            try
            {
                // Limite 5Mo
                long maxFileSize = 5 * 1024 * 1024;
                if (e.File.Size > maxFileSize)
                {
                    ErrorMessage = "L'image est trop lourde (Max 5Mo).";
                    return;
                }

                // Upload via le service
                // Note: Assure-toi que UploadCategory.Products correspond à ton Enum backend
                string url = await _uploadService.UploadImageAsync(e.File, UploadCategory.Products);

                if (!string.IsNullOrEmpty(url))
                {
                    Product.ProductPictureUrl = url;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur upload image : {ex.Message}";
            }
            finally
            {
                IsUploadingImage = false;
            }
        }

        public async Task SaveProductAsync()
        {
            if (IsSaving) return;
            IsSaving = true;
            ErrorMessage = null;

            try
            {
                // Validation manuelle de sécurité
                if (string.IsNullOrWhiteSpace(Product.Label) ||
                    Product.Price <= 0 ||
                    Product.ProductCategoryId <= 0)
                {
                    ErrorMessage = "Veuillez remplir les champs obligatoires correctement.";
                    IsSaving = false;
                    return;
                }

                if (IsEditMode)
                {
                    var updateDto = new ProductUpdateDto
                    {
                        Label = Product.Label,
                        Description = Product.Description,
                        Price = Product.Price,
                        ProductPictureUrl = Product.ProductPictureUrl,
                        ProductCategoryId = Product.ProductCategoryId
                    };

                    await _productService.UpdateAsync(ProductId.Value, updateDto);
                }
                else
                {
                    await _productService.AddAsync(Product);
                }

                // Succès -> Retour boutique
                NavigateBack();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors de la sauvegarde : {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        public void NavigateBack()
        {
            // Retourne à la boutique (ou à la liste admin si tu en as une)
            _navigation.NavigateTo("/boutique");
        }
    }
}