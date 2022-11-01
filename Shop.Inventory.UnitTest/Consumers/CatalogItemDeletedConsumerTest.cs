using FluentAssertions;
using MassTransit;
using Moq;
using Shop.Common.Contracts;
using Shop.Inventory.Service.Consumers;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.UnitTest.Consumers
{
    public class CatalogItemDeletedConsumerTest
    {
        Mock<IRepository<CatalogItem>> catalogItemRepository = new Mock<IRepository<CatalogItem>>();

        [Fact]
        public async void Consume_WithNoItem_ReturnsNone()
        {
            // Arrange
            var catalogItemDeleted = CreateCatalogItemDeleted();
            var context = Mock.Of<ConsumeContext<CatalogItemDeleted>>(con => con.Message == catalogItemDeleted);
            catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((CatalogItem)null);

            CatalogItemDeletedConsumer consumer = new CatalogItemDeletedConsumer(catalogItemRepository.Object);
            // Act
            var result = consumer.Consume(context);
            // Assert
            result.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async void Consume_Withtem()
        {
            // Arrange
            var catalogItemDeleted = CreateCatalogItemDeleted();
            var catalogItem = CreateCatalogItem();
            var context = Mock.Of<ConsumeContext<CatalogItemDeleted>>(con => con.Message == catalogItemDeleted);
            catalogItemRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync(catalogItem);

            CatalogItemDeletedConsumer consumer = new CatalogItemDeletedConsumer(catalogItemRepository.Object);
            // Act
            var result = consumer.Consume(context);
            // Assert
            result.IsCompleted.Should().BeTrue();
        }

        private CatalogItemDeleted CreateCatalogItemDeleted()
        {
            return new CatalogItemDeleted(Guid.NewGuid());
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
