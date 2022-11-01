using Polly;
using Polly.Timeout;
using Shop.Common.MassTransit;
using Shop.Common.MongoDB;
using Shop.Inventory.Service.Clients;
using Shop.Inventory.Service.Entities;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static void Main(string[] args)
    {        
        var builder = WebApplication.CreateBuilder(args);

        const string AllowedOriginSettings = "AllowedOrigin";

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddMongo()
            .AddMongoRepository<InventoryItem>("inventoryitems")
            .AddMongoRepository<CatalogItem>("catalogitems")
            .AddMassTransitWithRabbitMq();

        AddCatalogClient(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(cors =>
            {
                cors.WithOrigins(builder.Configuration[AllowedOriginSettings])
                    .AllowAnyHeader().AllowAnyMethod();
            });
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

        static void AddCatalogClient(WebApplicationBuilder builder)
        {
            Random jitterer = new Random();

            builder.Services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://catalog-clusterip-srv:80");
            })
            .AddTransientHttpErrorPolicy(builders => builders.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcom, timespan, retryAttemp) =>
                {
                    var serviceProvider = builder.Services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttemp}");
                }
            ))
            .AddTransientHttpErrorPolicy(builders => builders.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    var serviceProvider = builder.Services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Opening the circuit from {timespan.TotalSeconds} seconds...");
                },
                onReset: () =>
                {
                    var serviceProvider = builder.Services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Closing the circuit...");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }
    }
}