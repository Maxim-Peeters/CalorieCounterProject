using CalorieCounter.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieCounter.Services
{
    public class ProductDataService
    {
        // Get all days and their products from the API
        public async Task<List<Day>> GetDaysAsync()
        {
            return await ApiService<List<Day>>.GetAsync("Calories");
        }

        // Get products for a specific date from the list of days
        public async Task<List<Product>> GetProductsForDateAsync(DateTime date)
        {
            var days = await GetDaysAsync();
            var day = days.FirstOrDefault(d => d.Date.Date == date.Date);
            return day?.Products ?? new List<Product>();
        }

        // Get total calories for a specific date
        public async Task<int> GetTotalCaloriesForDateAsync(DateTime date)
        {
            var products = await GetProductsForDateAsync(date);
            return products.Sum(p => p.TotalCalories);
        }

        // Delete a product
        
    }
}
