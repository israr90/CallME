using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private ApplicationDbContext _context;
        private DbSet<TEntity> _set;

        internal Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        protected DbSet<TEntity> Set
        {
            get { return _set ?? (_set = _context.Set<TEntity>()); }
        }

        public int Count()
        {
            return Set.Count();
        }

        public async Task<int> CountAsync()
        {
            return await Set.CountAsync();
        }

        public List<TEntity> GetAll(bool enable = true)
        {
            this._context.Configuration.LazyLoadingEnabled = enable;
            this._context.Configuration.ProxyCreationEnabled = enable;

            return Set?.ToList();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await Set.ToListAsync();
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await Set.ToListAsync(cancellationToken);
        }

        public TEntity FindById(object id, bool enable = true)
        {
            this._context.Configuration.LazyLoadingEnabled = enable;
            this._context.Configuration.ProxyCreationEnabled = enable;
            return Set.Find(id);
        }

        public async Task<TEntity> FindByIdAsync(object id)
        {
            return await Set.FindAsync(id);
        }

        public async Task<TEntity> FindByIdAsync(CancellationToken cancellationToken, object id)
        {
            return await Set.FindAsync(cancellationToken, id);
        }

        public void Add(TEntity entity)
        {
            Set.Add(entity);
        }

        public void Update(TEntity entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                Set.Attach(entity);
                entry = _context.Entry(entity);
            }
            entry.State = EntityState.Modified;
        }

        public void Remove(TEntity entity)
        {
            Set.Remove(entity);
        }

        public void DisableLazyLoading(bool enable)
        {
            _context.Configuration.LazyLoadingEnabled = enable;
            _context.Configuration.ProxyCreationEnabled = enable;
        }
    }
}
