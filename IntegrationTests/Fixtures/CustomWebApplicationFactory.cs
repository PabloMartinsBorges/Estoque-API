using Estoque.Infra.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot;

namespace IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remove TODAS as configurações anteriores do DbContext (Npgsql + EF Core 9/10 configs)
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                d.ServiceType.Name.Contains("IDbContextOptionsConfiguration")
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // 2. Registra o DbContext limpo usando o provedor In-Memory exclusivo para os testes
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("EstoqueIntegrationTestsDb");
            });

            // 3. Substitui o cliente do Telegram por um Mock
            var tgDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ITelegramBotClient));

            if (tgDescriptor != null)
                services.Remove(tgDescriptor);

            var tgMock = new Mock<ITelegramBotClient>();
            services.AddSingleton(tgMock.Object);

            // 4. Inicializa o schema limpo no banco em memória
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}