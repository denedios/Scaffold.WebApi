namespace Scaffold.Application.UnitTests.Features.Bucket
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Scaffold.Application.Features.Bucket;
    using Scaffold.Application.Repositories;
    using Scaffold.Data;
    using Scaffold.Domain.Entities;
    using Xunit;

    public class RemoveBucketUnitTests
    {
        private readonly IBucketRepository repository;

        private readonly BucketContext context;

        public RemoveBucketUnitTests()
        {
            this.context = new BucketContext(new DbContextOptionsBuilder<BucketContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            this.repository = new BucketRepository(this.context);
        }

        public class Handler : RemoveBucketUnitTests
        {
            [Fact]
            public async Task When_RemovingExistingBucket_Expect_Removed()
            {
                // Arrange
                Bucket bucket = new Bucket();
                this.repository.Add(bucket);

                RemoveBucket.Command command = new RemoveBucket.Command { Id = bucket.Id };
                RemoveBucket.Handler handler = new RemoveBucket.Handler(this.repository);

                // Act
                await handler.Handle(command, default(CancellationToken));

                // Assert
                Assert.Null(this.repository.Get(bucket.Id));
            }

            [Fact]
            public async Task When_RemovingNonExistingBucket_Expect_NoChange()
            {
                // Arrange
                Bucket bucket = new Bucket();
                this.repository.Add(bucket);

                RemoveBucket.Command command = new RemoveBucket.Command { Id = new Random().Next(int.MaxValue) };
                RemoveBucket.Handler handler = new RemoveBucket.Handler(this.repository);

                // Act
                await handler.Handle(command, default(CancellationToken));

                // Assert
                Assert.NotEmpty(this.context.Buckets);
            }
        }
    }
}
