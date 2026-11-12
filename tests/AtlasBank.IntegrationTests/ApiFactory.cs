using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace AtlasBank.IntegrationTests
{
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16").Build();

        public async Task InitializeAsync() => await _db.StartAsync();

        public new async Task DisposeAsync() => await _db.DisposeAsync();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
            builder.UseSetting("Jwt:SecretKey", "chave-de-teste-minimo-32-caracteres-xyz");
        }        
    }

    [CollectionDefinition("api")]
    public class ApiCollection : ICollectionFixture<ApiFactory> { }
}