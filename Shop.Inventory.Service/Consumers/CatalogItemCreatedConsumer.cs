using MassTransit;
using Shop.Common.Contracts;
using Shop.Common;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.Service.Consumers;

public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
{

    private readonly IRepository<CatalogItem> repo;

    public CatalogItemCreatedConsumer(IRepository<CatalogItem> repo)
    {
        this.repo = repo;
    }

    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        var message = context.Message;
        var item = await repo.GetAsync(message.ItemId);

        if (item is not null)
        {
            return;
        }

        item = new CatalogItem
        {
            Id = message.ItemId,
            Name = message.Name,
            Description = message.Description
        };

        await repo.CreateAsync(item);
    }
}