using DAL.Data;
using DAL.Models;

namespace DAL.Repositories
{
    public class DayRepository : GenericRepository<Day>
    {
        public readonly CalorieContext _context;
        public DayRepository(CalorieContext context) : base(context)
        {
            _context = context;
        }
    }
}
