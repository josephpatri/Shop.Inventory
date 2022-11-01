using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using Shop.Inventory.Service.DTOs;

namespace Shop.Inventory.Service.Clients;

[ExcludeFromCodeCoverage]
public class CatalogClient
{
    private readonly HttpClient httpClient;

    public CatalogClient(HttpClient _httpClient)
    {
        httpClient = _httpClient;
    }

    /// <summary>
    /// We excluse this class because we use asyncronous communication
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
    {
        var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("http://localhost/items");
        return items!;
    }
}