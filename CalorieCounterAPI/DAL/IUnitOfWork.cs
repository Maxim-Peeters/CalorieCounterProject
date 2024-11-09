using DAL.Models;
using DAL.Repositories;

namespace DAL
{
    public interface IUnitOfWork
    {
        IRepository<Product> ProductRepository { get; }
        IRepository<Day> DayRepository { get; }
        Task SaveAsync();
    }
}
