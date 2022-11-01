namespace Shop.Inventory.Service.DTOs;

public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);
public record InventoryItemDto(Guid CatalogItemId, string Name, string Descripcion, int Quantity, DateTimeOffset AcquiredDate);
public record CatalogItemDto(Guid Id, string Name, string Description);
