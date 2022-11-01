using MassTransit;
using Shop.Common.Contracts;
using Shop.Common;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.Service.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{

    private readonly IRepository<CatalogItem> repo;

    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repo)
    {
        this.repo = repo;
    }

    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;
        var item = await repo.GetAsync(message.ItemId);

        if (item is null)
        {
            return;
        }
        await repo.RemoveAsync(message.ItemId);
    }
}