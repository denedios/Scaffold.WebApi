namespace Scaffold.WebApi.IntegrationTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Scaffold.Repositories.PostgreSQL;
    using Scaffold.WebApi.Views;
    using Xunit;

    public class BucketsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        public BucketsControllerIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        private HttpClient CreateNewTestClient()
        {
            string databaseName = Guid.NewGuid().ToString();

            return this.factory.WithWebHostBuilder(builder =>
            {
                builder
                    .ConfigureLogging(logging => logging.ClearProviders())
                    .ConfigureServices(services =>
                    {
                        ServiceDescriptor service = services.SingleOrDefault(service =>
                            service.ServiceType == typeof(DbContextOptions<BucketContext>));

                        if (service != null)
                        {
                            services.Remove(service);
                        }

                        services.AddDbContext<BucketContext>(options =>
                            options.UseInMemoryDatabase(databaseName));
                    });
            }).CreateClient();
        }

        public class AddBucket : BucketsControllerIntegrationTests
        {
            public AddBucket(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_AddingBucket_Expect_Created()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Size = new Random().Next(int.MaxValue),
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PostAsync("/Buckets", content);

                Bucket result = JsonSerializer.Deserialize<Bucket>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);
                Assert.Equal(new Uri($"http://localhost/Buckets/{result.Id}"), response.Headers.Location);

                Assert.Equal(bucket.Name, result.Name);
                Assert.Equal(bucket.Description, result.Description);
                Assert.Equal(bucket.Size, result.Size);
            }

            [Fact]
            public async Task When_AddingBucket_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket { Size = new Random().Next(int.MaxValue) };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket).Replace($"{bucket.Size}", "abc"),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PostAsync("/Buckets", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_AddingBucket_Expect_Conflict()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket { Size = -new Random().Next(int.MaxValue) };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PostAsync("/Buckets", content);

                // Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class GetBuckets : BucketsControllerIntegrationTests
        {
            public GetBuckets(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_GettingBuckets_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                int size = 10;

                for (int i = 1; i <= size; i++)
                {
                    Bucket bucket = new Bucket
                    {
                        Name = $"Bucket {i}",
                        Size = i,
                    };

                    StringContent content = new StringContent(
                        JsonSerializer.Serialize(bucket),
                        Encoding.UTF8,
                        MediaTypeNames.Application.Json);

                    await client.PostAsync("/Buckets", content);
                }

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets");

                List<Bucket> result = JsonSerializer.Deserialize<List<Bucket>>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.NotEmpty(result);
                Assert.Equal(size, result.Count);

                for (int i = 1; i <= size; i++)
                {
                    Assert.Equal($"Bucket {i}", result[i - 1].Name);
                    Assert.Equal(i, result[i - 1].Size);
                }
            }

            [Fact]
            public async Task When_GettingBuckets_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets?limit=abc&offset=xyz");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class GetBucket : BucketsControllerIntegrationTests
        {
            public GetBucket(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_GettingBucket_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Size = new Random().Next(int.MaxValue),
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{new Random().Next(int.MaxValue)}", content);

                // Act
                response = await client.GetAsync(response.Headers.Location);

                Bucket result = JsonSerializer.Deserialize<Bucket>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.Equal(bucket.Name, result.Name);
                Assert.Equal(bucket.Description, result.Description);
                Assert.Equal(bucket.Size, result.Size);
            }

            [Fact]
            public async Task When_GettingBucket_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets/abc");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_GettingBucket_Expect_NotFound()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets/123");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class UpdateBucket : BucketsControllerIntegrationTests
        {
            public UpdateBucket(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_UpdatingBucket_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{new Random().Next(int.MaxValue)}", content);

                // Act
                Bucket updatedBucket = new Bucket
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Size = new Random().Next(int.MaxValue),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(updatedBucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync(response.Headers.Location, content);

                Bucket result = JsonSerializer.Deserialize<Bucket>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.Equal(updatedBucket.Name, result.Name);
                Assert.Equal(updatedBucket.Description, result.Description);
                Assert.Equal(updatedBucket.Size, result.Size);
            }

            [Fact]
            public async Task When_UpdatingBucket_Expect_Created()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Size = new Random().Next(int.MaxValue),
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PutAsync($"/Buckets/{new Random().Next(int.MaxValue)}", content);

                Bucket result = JsonSerializer.Deserialize<Bucket>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);
                Assert.Equal(new Uri($"http://localhost/Buckets/{result.Id}"), response.Headers.Location);

                Assert.Equal(bucket.Name, result.Name);
                Assert.Equal(bucket.Description, result.Description);
                Assert.Equal(bucket.Size, result.Size);
            }

            [Fact]
            public async Task When_UpdatingBucket_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket { Size = new Random().Next(int.MaxValue) };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket).Replace($"{bucket.Size}", "abc"),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PutAsync("/Buckets/abc", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_UpdatingBucket_Expect_Conflict()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PostAsync("/Buckets", content);

                // Act
                Bucket updatedBucket = new Bucket
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Size = -new Random().Next(int.MaxValue),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(updatedBucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync(response.Headers.Location, content);

                // Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class RemoveBucket : BucketsControllerIntegrationTests
        {
            public RemoveBucket(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_RemovingBucket_Expect_NoContent()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{new Random().Next(int.MaxValue)}", content);

                // Act
                response = await client.DeleteAsync(response.Headers.Location);

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            [Fact]
            public async Task When_RemovingBucket_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.DeleteAsync("/Buckets/abc");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_RemovingBucket_Expect_NotFound()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.DeleteAsync("/Buckets/123");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class AddItem : BucketsControllerIntegrationTests
        {
            public AddItem(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_AddingItem_Expect_Created()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 1,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                // Act
                Item item = new Item
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PostAsync($"{response.Headers.Location}/Items", content);

                Item result = JsonSerializer.Deserialize<Item>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);
                Assert.Equal(new Uri($"http://localhost/Buckets/{bucket.Id}/Items/{result.Id}"), response.Headers.Location);

                Assert.Equal(item.Name, result.Name);
                Assert.Equal(item.Description, result.Description);
            }

            [Fact]
            public async Task When_AddingItem_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Item item = new Item { Name = "abc" };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(item).Replace("\"abc\"", "123"),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PostAsync("/Buckets/abc/Items", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_AddingItem_Expect_Conflict()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 0,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                // Act
                Item item = new Item();

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PostAsync($"{response.Headers.Location}/Items", content);

                // Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_AddingItem_Expect_NotFound()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Item item = new Item();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PostAsync("/Buckets/123/Items", content);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class GetItems : BucketsControllerIntegrationTests
        {
            public GetItems(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_GettingItems_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 10,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                for (int i = 1; i <= bucket.Size; i++)
                {
                    Item item = new Item { Name = $"Item {i}" };

                    content = new StringContent(
                        JsonSerializer.Serialize(item),
                        Encoding.UTF8,
                        MediaTypeNames.Application.Json);

                    await client.PostAsync($"{response.Headers.Location}/Items", content);
                }

                // Act
                response = await client.GetAsync($"{response.Headers.Location}/Items");

                List<Item> result = JsonSerializer.Deserialize<List<Item>>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.NotEmpty(result);
                Assert.Equal(bucket.Size, result.Count);

                for (int i = 1; i <= bucket.Size; i++)
                {
                    Assert.Equal($"Item {i}", result[i - 1].Name);
                }
            }

            [Fact]
            public async Task When_GettingItems_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets/abc/Items");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_GettingItems_Expect_NotFound()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets/123/Items");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class GetItem : BucketsControllerIntegrationTests
        {
            public GetItem(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_GettingItem_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 1,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                Item item = new Item
                {
                    Id = new Random().Next(int.MaxValue),
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}/Items/{item.Id}", content);

                // Act
                response = await client.GetAsync(response.Headers.Location);

                Item result = JsonSerializer.Deserialize<Item>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.Equal(item.Name, result.Name);
                Assert.Equal(item.Description, result.Description);
            }

            [Fact]
            public async Task When_GettingItem_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.GetAsync("/Buckets/abc/Items/abc");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Theory]
            [InlineData("/Buckets/123/Items/321")]
            [InlineData("/Buckets/321/Items/123")]
            public async Task When_GettingItem_Expect_NotFound(string path)
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                await client.PutAsync("/Buckets/123", content);

                Item item = new Item();

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                await client.PutAsync("/Buckets/123/Items/123", content);

                // Act
                HttpResponseMessage response = await client.GetAsync(path);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class UpdateItem : BucketsControllerIntegrationTests
        {
            public UpdateItem(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_UpdatingItem_Expect_Ok()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 1,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                Item item = new Item { Id = new Random().Next(int.MaxValue) };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}/Items/{item.Id}", content);

                // Act
                Item updatedItem = new Item
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(updatedItem),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}", content);

                Item result = JsonSerializer.Deserialize<Item>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);

                Assert.Equal(updatedItem.Name, result.Name);
                Assert.Equal(updatedItem.Description, result.Description);
            }

            [Fact]
            public async Task When_UpdatingItem_Expect_Created()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 1,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                // Act
                Item item = new Item
                {
                    Id = new Random().Next(int.MaxValue),
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}/Items/{item.Id}", content);

                Item result = JsonSerializer.Deserialize<Item>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType.MediaType);
                Assert.Equal(new Uri($"http://localhost/Buckets/{bucket.Id}/Items/{result.Id}"), response.Headers.Location);

                Assert.Equal(item.Name, result.Name);
                Assert.Equal(item.Description, result.Description);
            }

            [Fact]
            public async Task When_UpdatingItem_Expect_Conflict()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 0,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                // Act
                Item item = new Item { Id = new Random().Next(int.MaxValue) };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}/Items/{item.Id}", content);

                // Assert
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_UpdatingItem_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Item item = new Item { Name = "abc" };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(item).Replace("\"abc\"", "123"),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PutAsync("/Buckets/abc/Items/abc", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Fact]
            public async Task When_UpdatingItem_Expect_NotFound()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Item item = new Item();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                // Act
                HttpResponseMessage response = await client.PutAsync("/Buckets/123/Items/123", content);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }

        public class RemoveItem : BucketsControllerIntegrationTests
        {
            public RemoveItem(WebApplicationFactory<Startup> factory)
                : base(factory)
            {
            }

            [Fact]
            public async Task When_RemovingItem_Expect_NoContent()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket
                {
                    Id = new Random().Next(int.MaxValue),
                    Size = 1,
                };

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                HttpResponseMessage response = await client.PutAsync($"/Buckets/{bucket.Id}", content);

                Item item = new Item { Id = new Random().Next(int.MaxValue) };

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                response = await client.PutAsync($"{response.Headers.Location}/Items/{item.Id}", content);

                // Act
                response = await client.DeleteAsync(response.Headers.Location);

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            [Fact]
            public async Task When_RemovingItem_Expect_BadRequest()
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                // Act
                HttpResponseMessage response = await client.DeleteAsync("/Buckets/abc/Items/abc");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }

            [Theory]
            [InlineData("/Buckets/123/Items/321")]
            [InlineData("/Buckets/321/Items/123")]
            public async Task When_RemovingItem_Expect_NotFound(string path)
            {
                // Arrange
                HttpClient client = this.CreateNewTestClient();

                Bucket bucket = new Bucket();

                StringContent content = new StringContent(
                    JsonSerializer.Serialize(bucket),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                await client.PutAsync("/Buckets/123", content);

                Item item = new Item();

                content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);

                await client.PutAsync("/Buckets/123/Items/123", content);

                // Act
                HttpResponseMessage response = await client.DeleteAsync(path);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(CustomMediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);
            }
        }
    }
}
