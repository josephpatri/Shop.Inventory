using Shop.Inventory.Service.DTOs;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem item, string Name, string Description)
    {
        return new InventoryItemDto(item.CatalogItemId, Name, Description, item.Quantity, item.AcquiredDate);
    }
}
