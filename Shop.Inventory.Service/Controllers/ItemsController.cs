using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shop.Common;
using Shop.Inventory.Service.Clients;
using Shop.Inventory.Service.DTOs;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.Service.Controllers;

[Controller]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> inventoryitemsrepo;

    private readonly IRepository<CatalogItem> catalogitemsrepo;

    public ItemsController(IRepository<InventoryItem> _inventoryitemsrepo, IRepository<CatalogItem> _catalogitemsrepo)
    {
        inventoryitemsrepo = _inventoryitemsrepo;
        catalogitemsrepo = _catalogitemsrepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var inventoryItemEntities = await inventoryitemsrepo.GetAllAsync(item => item.UserId == userId);
        var itemsIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await catalogitemsrepo.GetAllAsync(item => itemsIds.Contains(item.Id));


        var inventoryItemsDtos = inventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemEntities.Single(ci => ci.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemsDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await inventoryitemsrepo.GetAsync(item => item.UserId == grantItemsDto.UserId
            && item.CatalogItemId == grantItemsDto.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                Id = Guid.NewGuid(),
                CatalogItemId = grantItemsDto.CatalogItemId,
                UserId = grantItemsDto.UserId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await inventoryitemsrepo.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await inventoryitemsrepo.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}
