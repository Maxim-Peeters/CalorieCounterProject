using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CalorieCounter.Services
{
    public static class ApiService<T>
    {
        private static readonly string BASE_URL = "https://localhost:7149/api/";
        static HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

        // GET request for retrieving data
        public static async Task<T> GetAsync(string endPoint)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        return JsonConvert.DeserializeObject<T>(jsonData);
                    }
                    else
                    {
                        throw new Exception("Resource Not Found");
                    }
                }
                else
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error during GET request: " + ex.Message);
            }
        }

        // POST request for sending data
        public static async Task PostAsync(string endPoint, T data)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var response = await client.PostAsJsonAsync(url, data);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error during POST request: " + ex.Message);
            }
        }

        // PUT request for updating data
        public static async Task PutAsync(string endPoint, T data)
        {
            try
            {
                string url = BASE_URL + endPoint;
                var response = await client.PutAsJsonAsync(url, data);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error during PUT request: " + ex.Message);
            }
        }

        // DELETE request for deleting data
        public static async Task<bool> DeleteAsync(string endPoint)
        {
            string url = BASE_URL + endPoint;
            try
            {
                var response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error during DELETE request: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error during DELETE request: " + ex.Message);
            }
        }
    }
}
