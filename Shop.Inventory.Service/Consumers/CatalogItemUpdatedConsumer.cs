using MassTransit;
using Shop.Common.Contracts;
using Shop.Common;
using Shop.Inventory.Service.Entities;

namespace Shop.Inventory.Service.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{

    private readonly IRepository<CatalogItem> repo;

    public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repo)
    {
        this.repo = repo;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        var message = context.Message;
        var item = await repo.GetAsync(message.ItemId);

        if (item is null)
        {
            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await repo.CreateAsync(item);
        }
        else
        {
            item.Name = message.Name;
            item.Description = message.Description;

            await repo.UpdateAsync(item);
        }
    }
}