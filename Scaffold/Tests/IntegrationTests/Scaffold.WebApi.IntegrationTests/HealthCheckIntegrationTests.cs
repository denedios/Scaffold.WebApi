namespace Scaffold.WebApi.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Scaffold.Repositories.PostgreSQL;
    using Xunit;

    public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        public HealthCheckIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory.WithWebHostBuilder(builder =>
                builder.ConfigureLogging(logging => logging.ClearProviders()));
        }

        [Fact]
        public async Task When_DatabaseIsAvailable_Expect_Ok()
        {
            // Arrange
            HttpClient client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor service = services.SingleOrDefault(service =>
                        service.ServiceType == typeof(DbContextOptions<BucketContext>));

                    services.Remove(service);

                    services.AddDbContext<BucketContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                });

                builder.UseSetting("HEALTHCHECKPORT", "80");
            }).CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task When_DatabaseIsUnavailable_Expect_ServiceUnavailable()
        {
            // Arrange
            HttpClient client = this.factory
                .WithWebHostBuilder(builder => builder.UseSetting("HEALTHCHECKPORT", "80"))
                .CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task When_FilteringByPort_Expect_OkOnHealthCheckPort()
        {
            // Arrange
            int healthCheckPort = new Random().Next(1024, 65535);

            HttpClient client = this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor service = services.SingleOrDefault(service =>
                        service.ServiceType == typeof(DbContextOptions<BucketContext>));

                    services.Remove(service);

                    services.AddDbContext<BucketContext>(options =>
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                });

                builder.UseSetting("HEALTHCHECKPORT", healthCheckPort.ToString());
                builder.UseSetting("URLS", $"http://+:80;http://+:{healthCheckPort}");
            }).CreateClient();

            // Act
            HttpResponseMessage expectedNotFoundResponse = await client.GetAsync("http://localhost/health");
            HttpResponseMessage expectedOkResponse = await client.GetAsync($"http://localhost:{healthCheckPort}/health");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, expectedNotFoundResponse.StatusCode);

            Assert.Equal(HttpStatusCode.OK, expectedOkResponse.StatusCode);
            Assert.Equal(MediaTypeNames.Text.Plain, expectedOkResponse.Content.Headers.ContentType.MediaType);
            Assert.Equal("Healthy", await expectedOkResponse.Content.ReadAsStringAsync());
        }
    }
}
