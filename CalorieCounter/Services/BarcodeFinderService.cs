using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalorieCounter.Helpers;

namespace CalorieCounter.Services
{
    public static class ApiKeysFinder
    {
        public static string CustomVisionEndPoint => "https://dvbarcodefinder-prediction.cognitiveservices.azure.com";
        public static string PredictionKey => "471bef8fd9a9494bb56fe1a722d30396";
        public static string ProjectId => "a7e5e1ec-bfe9-4582-b871-cde0d2cded11";
        public static string PublishedName => "Barcode I3";
    }

    public class BarcodeFinderService
    {
        public static double MinProbability => 0.9;

        // Modify the method to accept file path instead of Stream
        public static async Task<string?> ProcessImageAndGetBarcodeAsync(string photoPath)
        {
            try
            {
                // Ensure the file exists before processing
                if (!File.Exists(photoPath))
                    throw new FileNotFoundException($"File not found: {photoPath}");

                // Get the prediction from the model
                var prediction = await DetectObjectAsync(photoPath);
                if (prediction != null && prediction.Probability > MinProbability)
                {
                    // Create a temporary file to save the cropped image
                    string tempOutputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_cropped.png");

                    // Use ImageHelper to crop the bounding box
                    ImageHelper.CropBoundingBox(photoPath, prediction.BoundingBox, tempOutputPath);

                    return tempOutputPath; // Return the path to the cropped image
                }

                // If no barcode detected or probability is too low, return null
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                throw;
            }
        }

        // Method to detect the barcode (prediction) in the image file path
        private static async Task<PredictionModel?> DetectObjectAsync(string photoPath)
        {
            try
            {
                var endpoint = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(ApiKeysFinder.PredictionKey))
                {
                    Endpoint = ApiKeysFinder.CustomVisionEndPoint
                };

                var fileStream = File.OpenRead(photoPath);

                var results = await endpoint.DetectImageAsync(
                    Guid.Parse(ApiKeysFinder.ProjectId),
                    ApiKeysFinder.PublishedName,
                    fileStream
                );

                return results.Predictions.OrderByDescending(x => x.Probability).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting object: {ex.Message}");
                throw;
            }
        }
    }
}
