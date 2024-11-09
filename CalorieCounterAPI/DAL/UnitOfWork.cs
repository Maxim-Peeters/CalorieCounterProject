using DAL.Data;
using DAL.Models;
using DAL.Repositories;

namespace DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        public CalorieContext _context;

        public IRepository<Product> productRepository;
        public IRepository<Day> dayRepository;

        public UnitOfWork(CalorieContext context)
        {
            _context = context;
        }

        public IRepository<Product> ProductRepository
        {
            get
            {
                if (productRepository == null)
                {
                    productRepository =
                        new GenericRepository<Product>(_context);
                }
                return productRepository;
            }
        }

        public IRepository<Day> DayRepository
        {
            get
            {
                if (dayRepository == null)
                {
                    dayRepository =
                        new GenericRepository<Day>(_context);
                }
                return dayRepository;
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
