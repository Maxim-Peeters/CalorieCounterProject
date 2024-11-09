using DAL.Data;
using DAL.Models;

namespace DAL.Repositories
{
    public class ProductRepository : GenericRepository<Product>
    {
        public readonly CalorieContext _context;
        public ProductRepository(CalorieContext context) : base(context)
        {
            _context = context;
        }
    }
}
