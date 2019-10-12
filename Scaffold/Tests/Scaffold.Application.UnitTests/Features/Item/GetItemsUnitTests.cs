namespace Scaffold.Application.UnitTests.Features.Item
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Scaffold.Application.Exceptions;
    using Scaffold.Application.Features.Item;
    using Scaffold.Application.Interfaces;
    using Scaffold.Domain.Aggregates.Bucket;
    using Scaffold.Repositories.EntityFrameworkCore;
    using Xunit;

    public class GetItemsUnitTests
    {
        private readonly IBucketRepository repository;

        public GetItemsUnitTests()
        {
            BucketContext context = new BucketContext(new DbContextOptionsBuilder<BucketContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            this.repository = new BucketRepository(context);
        }

        public class Response
        {
            [Fact]
            public void When_InstantiatingResponseWithItems_Expect_ResponseWithItems()
            {
                // Arrange
                List<Item> items = new List<Item>();

                // Act
                GetItems.Response response = new GetItems.Response(items);

                // Assert
                Assert.Equal(items, response.Items);
            }

            [Fact]
            public void When_InstantiatingResponseWithNull_Expect_ArgumentNullException()
            {
                // Act
                Exception exception = Record.Exception(() => new GetItems.Response(null!));

                // Assert
                Assert.IsType<ArgumentNullException>(exception);
            }
        }

        public class Handler : GetItemsUnitTests
        {
            [Fact]
            public async Task When_GettingItemsFromBucket_Expect_ExistingItems()
            {
                // Arrange
                Bucket bucket = new Bucket();
                bucket.AddItem(new Item());
                bucket.AddItem(new Item());
                bucket.AddItem(new Item());

                await this.repository.AddAsync(bucket);

                GetItems.Query query = new GetItems.Query { BucketId = bucket.Id };
                GetItems.Handler handler = new GetItems.Handler(this.repository);

                // Act
                GetItems.Response response = await handler.Handle(query, default);

                // Assert
                Assert.NotNull(response.Items);
                Assert.NotEmpty(response.Items);
                Assert.Equal(3, response.Items.Count);
            }

            [Fact]
            public async Task When_GettingNonExistingItemsFromBucket_Expect_EmptyList()
            {
                // Arrange
                Bucket bucket = new Bucket();
                await this.repository.AddAsync(bucket);

                GetItems.Query query = new GetItems.Query { BucketId = bucket.Id };
                GetItems.Handler handler = new GetItems.Handler(this.repository);

                // Act
                GetItems.Response response = await handler.Handle(query, default);

                // Assert
                Assert.NotNull(response.Items);
                Assert.Empty(response.Items);
            }

            [Fact]
            public async Task When_GettingItemsFromNonExistingBucket_Expect_BucketNotFoundException()
            {
                // Arrange
                GetItems.Query query = new GetItems.Query { BucketId = new Random().Next(int.MaxValue) };
                GetItems.Handler handler = new GetItems.Handler(this.repository);

                // Act
                Exception exception = await Record.ExceptionAsync(() =>
                    handler.Handle(query, default));

                // Assert
                Assert.IsType<BucketNotFoundException>(exception);
            }
        }
    }
}
