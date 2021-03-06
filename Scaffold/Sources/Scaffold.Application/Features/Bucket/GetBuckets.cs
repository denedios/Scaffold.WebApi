namespace Scaffold.Application.Features.Bucket
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Scaffold.Application.Interfaces;
    using Scaffold.Application.Models;
    using Scaffold.Domain.Aggregates.Bucket;

    public static class GetBuckets
    {
        public class Query : IRequest<Response>
        {
            public Query()
            {
                this.Predicate = bucket => true;
            }

            public Query(Expression<Func<Bucket, bool>> predicate)
            {
                this.Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            }

            public Query(Expression<Func<Bucket, bool>> predicate, int? limit, int? offset, Ordering<Bucket>? ordering)
                : this(predicate)
            {
                this.Limit = limit;
                this.Offset = offset;
                this.Ordering = ordering;
            }

            public Expression<Func<Bucket, bool>> Predicate { get; }

            public int? Limit { get; } = null;

            public int? Offset { get; } = null;

            public Ordering<Bucket>? Ordering { get; } = null;
        }

        public class Response
        {
            public Response(IList<Bucket> buckets)
            {
                this.Buckets = buckets ?? throw new ArgumentNullException(nameof(buckets));
            }

            public IList<Bucket> Buckets { get; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IBucketReadRepository repository;

            public Handler(IBucketReadRepository repository)
            {
                this.repository = repository;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                return new Response(await this.repository.GetAsync(request.Predicate, request.Limit, request.Offset, request.Ordering, cancellationToken));
            }
        }
    }
}
