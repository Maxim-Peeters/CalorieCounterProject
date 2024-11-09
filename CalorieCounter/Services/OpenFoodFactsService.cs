using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CalorieCounter.Services
{
    public class OpenFoodFactsService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetProductInfoByBarcode(string barcode)
        {
            string url = $"https://world.openfoodfacts.net/api/v2/product/{barcode}?fields=product_name,nutriments";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(responseBody);
                string productName = json["product"]?["product_name"]?.ToString();
                double calories = json["product"]?["nutriments"]?["energy-kcal_100g"]?.ToObject<double?>() ?? 0;

                return $"{productName};{calories}";
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }
}
