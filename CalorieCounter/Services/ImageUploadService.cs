using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CalorieCounter.Services
{
    public class ImageUploadService
    {
        private readonly string _apiUrl = "https://localhost:7183/api/upload";

        public async Task<(bool success, string error)> UploadImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Debug.WriteLine($"UploadImageAsync: File does not exist at specified path: {imagePath}");
                return (false, "File does not exist at specified path");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    using var content = new MultipartFormDataContent();
                    using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                    using var fileContent = new StreamContent(fileStream);

                    // Call the GetContentType method to get the content type
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
                        return (false, $"Upload failed. Status: {response.StatusCode}, Response: {responseBody}");
                    }

                    Debug.WriteLine("UploadImageAsync: Upload succeeded");
                    return (true, null);
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"UploadImageAsync: Network error: {ex.Message}");
                    return (false, $"Network error: {ex.Message}");
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("UploadImageAsync: Request timed out");
                    return (false, "Request timed out");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UploadImageAsync: Error uploading image: {ex}");
                    return (false, $"Unexpected error: {ex.Message}");
                }
            }
        }

        // Method to determine the content type based on the file extension
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
