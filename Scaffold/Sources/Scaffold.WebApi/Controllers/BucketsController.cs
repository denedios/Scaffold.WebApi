﻿namespace Scaffold.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Scaffold.Application.Features.Bucket;
    using Scaffold.WebApi.Views;

    [ApiController]
    [Route("[controller]")]
    public class BucketsController : ControllerBase
    {
        private readonly IMapper mapper;

        private readonly IMediator mediator;

        public BucketsController(IMapper mapper, IMediator mediator)
        {
            this.mapper = mapper;
            this.mediator = mediator;
        }

        /// <summary>Creates a bucket.</summary>
        /// <param name="bucket">A complete or partial set of key-value pairs to create the Bucket object with.</param>
        /// <returns>The created Bucket object.</returns>
        /// <response code="201">Bucket created successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Bucket))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> AddBucket([FromBody] Bucket bucket)
        {
            AddBucket.Command command = this.mapper.Map<AddBucket.Command>(bucket);
            AddBucket.Response response = await this.mediator.Send(command);

            bucket = this.mapper.Map<Bucket>(response.Bucket);

            return this.CreatedAtRoute("GetBucket", new { bucketId = bucket.Id }, bucket);
        }

        /// <summary>Retrieves a list of buckets.</summary>
        /// <param name="limit">The maximun number of buckets to return from the result set. Defaults to 10.</param>
        /// <param name="offset">The number of buckets to omit from the start of the result set.</param>
        /// <param name="cancellationToken">The cancellation token to check to see if the request has been aborted.</param>
        /// <returns>A list of Bucket objects.</returns>
        /// <response code="200">Buckets retrieved successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IList<Bucket>> GetBuckets([FromQuery]int? limit, [FromQuery]int? offset, CancellationToken cancellationToken)
        {
            GetBuckets.Query query = new GetBuckets.Query { Limit = limit, Offset = offset };
            GetBuckets.Response response = await this.mediator.Send(query, cancellationToken);

            return this.mapper.Map<List<Bucket>>(response.Buckets);
        }

        /// <summary>Retrieves a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token to check to see if the request has been aborted.</param>
        /// <returns>The specified Bucket object.</returns>
        /// <response code="200">Bucket retrieved successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet("{bucketId}", Name = "GetBucket")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<Bucket> GetBucket(int bucketId, CancellationToken cancellationToken)
        {
            GetBucket.Query query = new GetBucket.Query { Id = bucketId };
            GetBucket.Response response = await this.mediator.Send(query, cancellationToken);

            return this.mapper.Map<Bucket>(response.Bucket);
        }

        /// <summary>Updates a bucket or creates one if the specified one does not exist.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to be created or updated.</param>
        /// <param name="bucket">A complete set of key-value pairs to create or update the Bucket object with.</param>
        /// <returns>The created or updated Bucket object.</returns>
        /// <response code="200">Bucket updated successfully.</response>
        /// <response code="201">Bucket created successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpPut("{bucketId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Bucket))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Bucket>> UpdateBucket(int bucketId, [FromBody] Bucket bucket)
        {
            UpdateBucket.Command command = this.mapper.Map<UpdateBucket.Command>(bucket);
            command.Id = bucketId;

            UpdateBucket.Response response = await this.mediator.Send(command);
            bucket = this.mapper.Map<Bucket>(response.Bucket);

            if (response.Created)
            {
                return this.CreatedAtRoute("GetBucket", new { bucketId }, bucket);
            }

            return bucket;
        }

        /// <summary>Deletes a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to be deleted.</param>
        /// <returns>A "No Content (204)" HTTP status response.</returns>
        /// <response code="204">Bucket deleted successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpDelete("{bucketId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> RemoveBucket(int bucketId)
        {
            await this.mediator.Send(new RemoveBucket.Command { Id = bucketId });
            return this.NoContent();
        }

        /// <summary>Creates an item in a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to create the item in.</param>
        /// <param name="item">A complete or partial set of key-value pairs to create the Item object with.</param>
        /// <returns>The created Item object.</returns>
        /// <response code="201">Item created successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpPost("{bucketId}/Items")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Item))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> AddItem(int bucketId, [FromBody] Item item)
        {
            AddItem.Command command = this.mapper.Map<AddItem.Command>(item);
            command.BucketId = bucketId;

            AddItem.Response response = await this.mediator.Send(command);
            item = this.mapper.Map<Item>(response.Item);

            return this.CreatedAtRoute("GetItem", new { bucketId, itemId = item.Id }, item);
        }

        /// <summary>Retrieves a list of items from a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to retrieve the items from.</param>
        /// <param name="cancellationToken">The cancellation token to check to see if the request has been aborted.</param>
        /// <returns>A list of Item objects.</returns>
        /// <response code="200">Items retrieved successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet("{bucketId}/Items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IList<Item>> GetItems(int bucketId, CancellationToken cancellationToken)
        {
            GetItems.Query query = new GetItems.Query { BucketId = bucketId };
            GetItems.Response response = await this.mediator.Send(query, cancellationToken);

            return this.mapper.Map<List<Item>>(response.Items);
        }

        /// <summary>Retrieves an item from a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to retrieve the item from.</param>
        /// <param name="itemId">The Id. of the Item object to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token to check to see if the request has been aborted.</param>
        /// <returns>The specified Item object.</returns>
        /// <response code="200">Item retrieved successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpGet("{bucketId}/Items/{itemId}", Name = "GetItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<Item> GetItem(int bucketId, int itemId, CancellationToken cancellationToken)
        {
            GetItem.Query query = new GetItem.Query { BucketId = bucketId, ItemId = itemId };
            GetItem.Response response = await this.mediator.Send(query, cancellationToken);

            return this.mapper.Map<Item>(response.Item);
        }

        /// <summary>Updates an item in a bucket or creates one if the specified one does not exist.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to create or update the item in.</param>
        /// <param name="itemId">The Id. of the Item object to be created or updated.</param>
        /// <param name="item">A complete set of key-value pairs to create or update the Item object with.</param>
        /// <returns>The created or updated Item object.</returns>
        /// <response code="200">Item updated successfully.</response>
        /// <response code="201">Item created successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpPut("{bucketId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Item))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Item>> UpdateItem(int bucketId, int itemId, [FromBody] Item item)
        {
            UpdateItem.Command command = this.mapper.Map<UpdateItem.Command>(item);
            command.BucketId = bucketId;
            command.ItemId = itemId;

            UpdateItem.Response response = await this.mediator.Send(command);
            item = this.mapper.Map<Item>(response.Item);

            if (response.Created)
            {
                return this.CreatedAtRoute("GetItem", new { bucketId, itemId }, item);
            }

            return item;
        }

        /// <summary>Deletes an item in a bucket.</summary>
        /// <param name="bucketId">The Id. of the Bucket object to delete the item from.</param>
        /// <param name="itemId">The Id. of the Item object to be deleted.</param>
        /// <returns>A "No Content (204)" HTTP status response.</returns>
        /// <response code="204">Item deleted successfully.</response>
        /// <response code="default">Problem Details (RFC 7807) Response.</response>
        [HttpDelete("{bucketId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> RemoveItem(int bucketId, int itemId)
        {
            await this.mediator.Send(new RemoveItem.Command { BucketId = bucketId, ItemId = itemId });
            return this.NoContent();
        }
    }
}
