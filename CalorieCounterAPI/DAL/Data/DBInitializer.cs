using DAL.Models;

namespace DAL.Data
{
    public class DBInitializer
    {
        public static void Initialize(CalorieContext context)
        {
            context.Database.EnsureCreated();

            // Check if there is any existing data
            if (context.Days.Any())
            {
                return; // DB has already been seeded
            }

            // Create days from 3 days ago until the end of November
            DateTime startDate = DateTime.Now.Date.AddDays(-3);
            DateTime endDate = new DateTime(DateTime.Now.Year, 11, 30);

            var days = new List<Day>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Create a new day entry
                var day = new Day
                {
                    Date = date,
                };

                days.Add(day);
            }

            // Add days to the context
            context.Days.AddRange(days);

            context.SaveChanges();
        }
    }
}
