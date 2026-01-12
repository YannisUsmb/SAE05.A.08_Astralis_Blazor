using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Json;
namespace Astralis_BlazorApp.Services;

public interface ICartService
{
    CartDto Cart { get; }
    event Action OnChange;
    Task LoadCartAsync();
    Task AddToCartAsync(ProductListDto product, int quantity = 1);
    Task IncreaseQuantityAsync(CartItemDto item);
    Task DecreaseQuantityAsync(CartItemDto item);
    Task RemoveFromCartAsync(CartItemDto item);
    Task<string?> CheckoutAsync();

    Task ClearCartAsync();
}

public class CartService : ICartService
{
    private readonly ICartItemService _apiService;
    private readonly IJSRuntime _js;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly HttpClient _http;

    private const string LocalCartKey = "astralis_local_cart";

    public CartDto Cart { get; private set; } = new();

    public event Action? OnChange;

    public CartService(ICartItemService apiService, IJSRuntime js, AuthenticationStateProvider authStateProvider, HttpClient http)
    {
        _apiService = apiService;
        _js = js;
        _authStateProvider = authStateProvider;
        _http = http;
    }

    public async Task LoadCartAsync()
    {
        if (await IsUserAuthenticated())
        {
            try
            {
                var apiCart = await _apiService.GetMyCartAsync();
                if (apiCart != null) Cart = apiCart;
            }
            catch { }
        }
        else
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", LocalCartKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var localItems = JsonSerializer.Deserialize<List<CartItemDto>>(json);
                    Cart = new CartDto { Items = localItems ?? new List<CartItemDto>() };
                }
                else
                {
                    Cart = new CartDto();
                }
            }
            catch
            {
                Cart = new CartDto();
            }
        }
        NotifyStateChanged();
    }

    public async Task AddToCartAsync(ProductListDto product, int quantity = 1)
    {
        if (await IsUserAuthenticated())
        {
            var dto = new CartItemCreateDto { ProductId = product.Id, Quantity = quantity };
            await _apiService.AddAsync(dto);
        }
        else
        {
            var items = Cart.Items.ToList();
            var existing = items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                items.Add(new CartItemDto
                {
                    ProductId = product.Id,
                    ProductPictureUrl = product.ProductPictureUrl,
                    ProductLabel = product.Label,
                    UnitPrice = product.Price,
                    Quantity = quantity
                });
            }
            await SaveLocalCart(items);
        }
        await LoadCartAsync();
    }

    public async Task IncreaseQuantityAsync(CartItemDto item)
    {
        if (await IsUserAuthenticated())
        {
            int userId = await GetUserId();
            var dto = new CartItemUpdateDto { Quantity = item.Quantity + 1 };
            await _apiService.UpdateAsync(userId, item.ProductId, dto);
        }
        else
        {
            var items = Cart.Items.ToList();
            var localItem = items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (localItem != null)
            {
                localItem.Quantity++;
                await SaveLocalCart(items);
            }
        }
        await LoadCartAsync();
    }

    public async Task DecreaseQuantityAsync(CartItemDto item)
    {
        if (await IsUserAuthenticated())
        {
            int userId = await GetUserId();
            if (item.Quantity > 1)
            {
                var dto = new CartItemUpdateDto { Quantity = item.Quantity - 1 };
                await _apiService.UpdateAsync(userId, item.ProductId, dto);
            }
            else
            {
                await _apiService.DeleteAsync(userId, item.ProductId);
            }
        }
        else
        {
            var items = Cart.Items.ToList();
            var localItem = items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (localItem != null)
            {
                if (localItem.Quantity > 1)
                {
                    localItem.Quantity--;
                }
                else
                {
                    items.Remove(localItem);
                }
                await SaveLocalCart(items);
            }
        }
        await LoadCartAsync();
    }
    public async Task ClearCartAsync()
    {
        if (await IsUserAuthenticated())
        {
            int userId = await GetUserId();
            await _apiService.ClearCartAsync(userId);
        }
        else
        {
            Cart.Items.Clear();
            await _js.InvokeVoidAsync("localStorage.removeItem", LocalCartKey);
        }

        // On recharge l'état (le panier sera vide)
        await LoadCartAsync();
    }
    public async Task RemoveFromCartAsync(CartItemDto item)
    {
        if (await IsUserAuthenticated())
        {
            int userId = await GetUserId();
            await _apiService.DeleteAsync(userId, item.ProductId);
        }
        else
        {
            var items = Cart.Items.ToList();
            var localItem = items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (localItem != null)
            {
                items.Remove(localItem);
                await SaveLocalCart(items);
            }
        }
        await LoadCartAsync();
    }

    public async Task<string?> CheckoutAsync()
    {
        try
        {
            var response = await _http.PostAsJsonAsync("Payment/create-checkout-session", Cart.Items);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();
                return result?.Url;
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ERREUR PAIEMENT : {response.StatusCode} - {errorMsg}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION PAIEMENT : {ex.Message}");
            return null;
        }
    }

    private async Task SaveLocalCart(List<CartItemDto> items)
    {
        var json = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("localStorage.setItem", LocalCartKey, json);
    }

    private async Task<bool> IsUserAuthenticated()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated ?? false;
    }

    private async Task<int> GetUserId()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var idClaim = state.User.FindFirst(ClaimTypes.NameIdentifier);
        return (idClaim != null && int.TryParse(idClaim.Value, out int uid)) ? uid : 0;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

public class PaymentResponse
{
    public string Url { get; set; }
}