namespace DAL.Data
{
    public class DBInitializer
    {
        public static void Initialize(CalorieContext context)
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }
}
