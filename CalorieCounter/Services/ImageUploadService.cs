using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;  // for cross-platform file handling

namespace CalorieCounter.Services
{
    public class ImageUploadService
    {
        private readonly string _apiUrl = "https://192.168.0.237:7183/api/upload";

        public async Task<(bool success, string error, string body)> UploadImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Debug.WriteLine($"UploadImageAsync: File does not exist at specified path: {imagePath}");
                return (false, "File does not exist at specified path", null);
            }

            try
            {
                // Set up the HttpClient with handler to bypass SSL validation
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
                using var content = new MultipartFormDataContent();

                using var fileStream = File.OpenRead(imagePath);
                using var fileContent = new StreamContent(fileStream);

                string contentType = GetContentType(imagePath);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "file", Path.GetFileName(imagePath));

                var response = await client.PostAsync(_apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"Upload failed. Status: {response.StatusCode}, Response: {responseBody}", null);
                }

                return (true, null, responseBody);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Network error: {ex.Message}", null);
            }
            catch (TaskCanceledException)
            {
                return (false, "Request timed out", null);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }

}
