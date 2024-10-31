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
            // Check if file exists with cross-platform support
            if (!File.Exists(imagePath))
            {
                Debug.WriteLine($"UploadImageAsync: File does not exist at specified path: {imagePath}");
                return (false, "File does not exist at specified path", null);
            }

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                using var content = new MultipartFormDataContent();

                // Ensure cross-platform file access
                using var fileStream = File.OpenRead(imagePath);
                using var fileContent = new StreamContent(fileStream);

                // Set content type for image file
                string contentType = GetContentType(imagePath);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "file", Path.GetFileName(imagePath));

                Debug.WriteLine($"UploadImageAsync: Attempting to upload file: {imagePath}");
                Debug.WriteLine($"UploadImageAsync: Content Type: {contentType}");
                Debug.WriteLine($"UploadImageAsync: API URL: {_apiUrl}");

                var response = await client.PostAsync(_apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"UploadImageAsync: Status Code: {response.StatusCode}");
                Debug.WriteLine($"UploadImageAsync: Response Body: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"UploadImageAsync: Upload failed. Status: {response.StatusCode}, Response: {responseBody}");
                    return (false, $"Upload failed. Status: {response.StatusCode}, Response: {responseBody}", null);
                }

                Debug.WriteLine("UploadImageAsync: Upload succeeded");
                return (true, null, responseBody);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"UploadImageAsync: Network error: {ex.Message}");
                return (false, $"Network error: {ex.Message}", null);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("UploadImageAsync: Request timed out");
                return (false, "Request timed out", null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadImageAsync: Error uploading image: {ex}");
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        // Determine content type based on file extension
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
