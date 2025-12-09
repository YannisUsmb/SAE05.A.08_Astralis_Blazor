using System.Net.Http.Json;
using Astralis_BlazorApp.Services.Interfaces;
using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Implementations;

public class CartItemService(HttpClient httpClient) : ICartItemService
{
    private const string Controller = "CartItems";
    
    public async Task<CartItemDto?> GetByIdAsync(int userId, int productId)
    {
        // GET api/CartItems/{userId}/{productId}
        return await httpClient.GetFromJsonAsync<CartItemDto>($"{Controller}/{userId}/{productId}");
    }

    public async Task<List<CartItemDto>> GetAllAsync()
    {
        // GET api/CartItems
        List<CartItemDto>? entities = await httpClient.GetFromJsonAsync<List<CartItemDto>>(Controller);
        return entities ?? new List<CartItemDto>();
    }

    public async Task<CartDto?> GetMyCartAsync()
    {
        // GET api/CartItems/my-cart (Adapte la route si besoin, ex: "current", "me")
        return await httpClient.GetFromJsonAsync<CartDto>($"{Controller}/my-cart");
    }

    public async Task<CartItemDto?> AddAsync(CartItemCreateDto dto)
    {
        // POST api/CartItems
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(Controller, dto);
        response.EnsureSuccessStatusCode();

        CartItemDto? createdEntity = await response.Content.ReadFromJsonAsync<CartItemDto>();
        return createdEntity ?? throw new Exception("Error adding item to cart");
    }

    public async Task UpdateAsync(int userId, int productId, CartItemUpdateDto dto)
    {
        // PUT api/CartItems/{userId}/{productId}
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{Controller}/{userId}/{productId}", dto);
        response.EnsureSuccessStatusCode();
        
        // Ton interface retourne "Task" (void), donc on ne retourne rien ici, 
        // contrairement à ton exemple CelestialBody qui retournait l'objet.
    }

    public async Task DeleteAsync(int userId, int productId)
    {
        // DELETE api/CartItems/{userId}/{productId}
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/{userId}/{productId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearCartAsync(int userId)
    {
        // DELETE api/CartItems/user/{userId}
        // J'ai ajouté "/user/" pour différencier de la suppression d'un item unique
        HttpResponseMessage response = await httpClient.DeleteAsync($"{Controller}/user/{userId}");
        response.EnsureSuccessStatusCode();
    }
}