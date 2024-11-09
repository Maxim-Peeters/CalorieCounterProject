using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DAL.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly CalorieContext _context;
        private readonly DbSet<T> _table;

        public GenericRepository(CalorieContext context)
        {
            _context = context;
            _table = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _table;

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate,
                                    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                    bool trackChanges = true)
        {
            IQueryable<T> query = _context.Set<T>();

            // Apply tracking or no-tracking based on trackChanges
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            // Apply includes if provided
            if (include != null)
            {
                query = include(query);
            }

            // Apply the predicate
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> GetByIDAsync(int id)
        {
            return await _table.FindAsync(id);
        }

        public async Task InsertAsync(T obj)
        {
            await _table.AddAsync(obj);
        }

        public async Task DeleteAsync(int id)
        {
            T obj = await _table.FindAsync(id);
            if (obj != null)
            {
                _table.Remove(obj);
            }
        }

        public async Task UpdateAsync(T obj)
        {
            _table.Update(obj);
        }
    }

}
