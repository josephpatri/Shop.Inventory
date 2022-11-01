using Moq;
using Shop.Common.Contracts;
using Shop.Inventory.Service.Entities;
using MassTransit;
using Shop.Inventory.Service.Consumers;
using FluentAssertions;

namespace Shop.Inventory.UnitTest.Consumers;
public class CatalogItemCreatedConsumerTest
{

    Mock<IRepository<CatalogItem>> catalogItemRepository = new Mock<IRepository<CatalogItem>>();

    [Fact]
    public async void Consume_WithExistingItem_ReturnsNone()
    {
        //Arrange                
        var catalogItemContract = CreateCatalogItemContract();
        var catalogItemEntitie = CreateCatalogItemEntitie();

        var context = Mock.Of<ConsumeContext<CatalogItemCreated>>(con => con.Message == catalogItemContract);
        catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync(catalogItemEntitie);
        CatalogItemCreatedConsumer consumer = new CatalogItemCreatedConsumer(catalogItemRepository.Object);
        //Act
        var result = consumer.Consume(context);
        //Assert
        result.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async void Consume_WithNoExistingItem()
    {
        //Arrange                
        var catalogItemContract = CreateCatalogItemContract();
        var catalogItemEntitie = CreateCatalogItemEntitie();

        var context = Mock.Of<ConsumeContext<CatalogItemCreated>>(con => con.Message == catalogItemContract);
        catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((CatalogItem)null);
        CatalogItemCreatedConsumer consumer = new CatalogItemCreatedConsumer(catalogItemRepository.Object);
        //Act
        var result = consumer.Consume(context);
        //Assert
        result.IsCompleted.Should().BeTrue();
    }

    private CatalogItemCreated CreateCatalogItemContract()
    {
        return new CatalogItemCreated(Guid.NewGuid(), "N", "D");
    }

    private CatalogItem CreateCatalogItemEntitie()
    {
        return new CatalogItem()
        {
            Id = Guid.NewGuid(),
            Description = "D",
            Name = "N"
        };
    }
}

