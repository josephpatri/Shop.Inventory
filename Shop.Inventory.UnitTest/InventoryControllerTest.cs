using Moq;
using Shop.Inventory.Service.Controllers;
using Shop.Inventory.Service.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Shop.Inventory.Service;
using Shop.Inventory.Service.DTOs;
using System.Linq.Expressions;

namespace Shop.Inventory.UnitTest;

public class ItemsControllerTest
{
    Mock<IRepository<InventoryItem>> inventoryRepository = new Mock<IRepository<InventoryItem>>();
    Mock<IRepository<CatalogItem>> catalogRepository = new Mock<IRepository<CatalogItem>>();

    Guid catalogItemId = Guid.NewGuid();

    [Fact]
    public async void GetAsync_WithNoExistingItem_ReturnsBadRequest()
    {
        //Arrange
        var GuidEmpty = Guid.Empty;
        ItemsController controller = new ItemsController(inventoryRepository.Object, catalogRepository.Object);

        //Act
        var result = await controller.GetAsync(Guid.Empty);

        // Assert        
        result.Result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async void GetAsync_WithExistingItemId_ReturnsOkResult()
    {
        //Arrange        
        var inventoryItems = new[] { CreateInventoryItem(), CreateInventoryItem(), CreateInventoryItem() };
        var catalogItems = new[] { CreateCatalogItem() };
        var catalogItemDto = CreateCatalogItemDto();

        inventoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>())).ReturnsAsync(inventoryItems);
        catalogRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<CatalogItem, bool>>>())).ReturnsAsync(catalogItems);

        var inventoryItemDtos = inventoryItems.Select(inventoryItem =>
        {
            var catItem = catalogItems.Single(ci => ci.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catItem.Name, catItem.Description);
        });

        ItemsController controller = new ItemsController(inventoryRepository.Object, catalogRepository.Object);

        //Act
        var actualItems = await controller.GetAsync(Guid.NewGuid());

        var result = (actualItems.Result as OkObjectResult)!.Value as IEnumerable<InventoryItemDto>;
        //Assert        
        actualItems.Should().BeOfType<ActionResult<IEnumerable<InventoryItemDto>>>();
        actualItems.Should().NotBeNull();
        inventoryItemDtos.Should().BeEquivalentTo(result);
        result!.Any().Should().BeTrue();
    }

    [Fact]
    public async void PostAsync_WithNoItem_ReturnOk()
    {
        //Arrange
        var itemToCreate = CreateGrantItem();
        inventoryRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>())).ReturnsAsync((InventoryItem)null);
        ItemsController controller = new ItemsController(inventoryRepository.Object, catalogRepository.Object);
        //Act
        var result = await controller.PostAsync(itemToCreate);
        //Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async void PostAsync_WithExistingItem_ReturnOk()
    {
        //Arrange
        var existingInvItem = CreateInventoryItem();
        var itemToCreate = CreateGrantItem();
        inventoryRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<InventoryItem, bool>>>())).ReturnsAsync(existingInvItem);
        existingInvItem.Quantity += itemToCreate.Quantity;
        ItemsController controller = new ItemsController(inventoryRepository.Object, catalogRepository.Object);
        //Act
        var result = await controller.PostAsync(itemToCreate);
        //Assert
        result.Should().BeOfType<OkResult>();
    }

    private InventoryItem CreateInventoryItem()
    {
        return new InventoryItem()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AcquiredDate = DateTimeOffset.UtcNow,
            CatalogItemId = catalogItemId,
            Quantity = 1
        };
    }
    private CatalogItem CreateCatalogItem()
    {
        return new CatalogItem()
        {
            Id = catalogItemId,
            Description = "Des",
            Name = "Name"
        };
    }

    private CatalogItemDto CreateCatalogItemDto()
    {
        return new CatalogItemDto(Guid.NewGuid(), "N", "D");
    }

    private GrantItemsDto CreateGrantItem()
    {
        return new GrantItemsDto(Guid.NewGuid(), Guid.NewGuid(), 1);
    }
}