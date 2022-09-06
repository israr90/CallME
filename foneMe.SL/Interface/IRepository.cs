using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {
        List<TEntity> GetAll(bool enable = true);
        Task<List<TEntity>> GetAllAsync();
        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

        TEntity FindById(object id, bool enable = true);
        Task<TEntity> FindByIdAsync(object id);
        Task<TEntity> FindByIdAsync(CancellationToken cancellationToken, object id);

        int Count();
        Task<int> CountAsync();

        void Add(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void DisableLazyLoading(bool disabled);
    }
}
