namespace Scaffold.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Scaffold.Application.Interfaces;
    using Scaffold.Data;
    using Scaffold.Domain.Entities;

    public class BucketRepository : IBucketRepository
    {
        private readonly BucketContext context;

        public BucketRepository(BucketContext context) =>
            this.context = context;

        public void Add(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Add(bucket);
            this.context.SaveChanges();
        }

        public async Task AddAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Add(bucket);
            await this.context.SaveChangesAsync();
        }

        public Bucket Get(int id) =>
            this.context.Set<Bucket>()
                .Where(bucket => bucket.Id == id)
                .Include(bucket => bucket.Items)
                .SingleOrDefault();

        public IList<Bucket> GetAll() =>
            this.context.Set<Bucket>()
                .Include(bucket => bucket.Items)
                .ToList();

        public async Task<IList<Bucket>> GetAllAsync() =>
            await this.context.Set<Bucket>()
                .Include(bucket => bucket.Items)
                .ToListAsync();

        public async Task<Bucket> GetAsync(int id) =>
            await this.context.Set<Bucket>()
                .Where(bucket => bucket.Id == id)
                .Include(bucket => bucket.Items)
                .SingleOrDefaultAsync();

        public void Remove(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Remove(bucket);
            this.context.SaveChanges();
        }

        public async Task RemoveAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Remove(bucket);
            await this.context.SaveChangesAsync();
        }

        public void Update(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Update(bucket);
            this.context.SaveChanges();
        }

        public async Task UpdateAsync(Bucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            this.context.Set<Bucket>().Update(bucket);
            await this.context.SaveChangesAsync();
        }
    }
}