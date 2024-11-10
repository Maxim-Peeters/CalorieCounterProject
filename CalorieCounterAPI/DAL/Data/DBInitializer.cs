using DAL.Models;

namespace DAL.Data
{
    public class DBInitializer
    {
        public static void Initialize(CalorieContext context)
        {
            context.Database.EnsureCreated();

            // Check if there is any existing data
            

            // Create a specific test date: 09/11/2024
            DateTime testDate = new DateTime(2024, 11, 10);

            // Create a new day entry for 09/11/2024
            var testDay = new Day
            {
                Date = testDate,
                Products = new List<Product>
                {
                    new Product
                    {
                        Name = "Oatmeal",
                        CaloriesPer100g = 68,
                        AmountInGrams = 150,
                        Barcode = "1234567890123",
                        Category = Category.Breakfast
                    },
                    new Product
                    {
                        Name = "Chicken Sandwich",
                        CaloriesPer100g = 250,
                        AmountInGrams = 200,
                        Barcode = "9876543210987",
                        Category = Category.Lunch
                    },
                    new Product
                    {
                        Name = "Apple",
                        CaloriesPer100g = 52,
                        AmountInGrams = 180,
                        Barcode = "1112223334445",
                        Category = Category.Snack
                    },
                    new Product
                    {
                        Name = "Spaghetti Bolognese",
                        CaloriesPer100g = 150,
                        AmountInGrams = 300,
                        Barcode = "5556667778889",
                        Category = Category.Dinner
                    }
                }
            };

            // Add the test day to the context
            context.Days.Add(testDay);

            // Save changes to the database
            context.SaveChanges();
        }
    }
}
