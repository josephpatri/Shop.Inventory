using FluentAssertions;
using MassTransit;
using Moq;
using Shop.Common.Contracts;
using Shop.Inventory.Service.Consumers;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.UnitTest.Consumers
{
    public class CatalogItemUpdatedConsumerTest
    {
        Mock<IRepository<CatalogItem>> catalogItemRepository = new Mock<IRepository<CatalogItem>>();

        [Fact]
        public async void Consume_WithNoItem()
        {
            // Arrange
            var catalogItemUpdate = CreateCatalogItemUpdated();
            var catalogItem = CreateCatalogItem();
            var context = Mock.Of<ConsumeContext<CatalogItemUpdated>>(con => con.Message == catalogItemUpdate);
            catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((CatalogItem)null);
            CatalogItemUpdatedConsumer consumer = new CatalogItemUpdatedConsumer(catalogItemRepository.Object);
            // Act
            var result = consumer.Consume(context);
            // Assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async void Consume_WithItem()
        {
            // Arrange
            var catalogItemUpdate = CreateCatalogItemUpdated();
            var catalogItem = CreateCatalogItem();
            var context = Mock.Of<ConsumeContext<CatalogItemUpdated>>(con => con.Message == catalogItemUpdate);
            catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync(catalogItem);
            CatalogItemUpdatedConsumer consumer = new CatalogItemUpdatedConsumer(catalogItemRepository.Object);
            // Act
            var result = consumer.Consume(context);
            // Assert
            result.IsCompleted.Should().BeTrue();
        }

        private CatalogItemUpdated CreateCatalogItemUpdated()
        {
            return new CatalogItemUpdated(Guid.NewGuid(), "N", "D");
        }

        private CatalogItem CreateCatalogItem()
        {
            return new CatalogItem()
            {
                Id = Guid.NewGuid(),
                Name = "N",
                Description = "D"
            };
        }
    }
}
