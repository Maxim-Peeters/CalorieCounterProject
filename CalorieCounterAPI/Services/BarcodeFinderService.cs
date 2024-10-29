using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
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

        public static async Task<bool> ProcessImageAndGetBarcodeAsync(string inputImagePath, string outputCroppedPath)
        {
            try
            {
                using (var fileStream = new FileStream(inputImagePath, FileMode.Open))
                {
                    var prediction = await DetectObjectAsync(fileStream);

                    if (prediction != null && prediction.Probability > MinProbability)
                    {
                        ImageHelper.CropBoundingBox(inputImagePath, prediction.BoundingBox, outputCroppedPath);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                throw;
            }
            finally
            {
                if (File.Exists(inputImagePath))
                {
                    try
                    {
                        File.Delete(inputImagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting temporary file: {ex.Message}");
                    }
                }
            }
        }

        private static async Task<PredictionModel> DetectObjectAsync(Stream photoStream)
        {
            try
            {
                var endpoint = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(ApiKeysFinder.PredictionKey))
                {
                    Endpoint = ApiKeysFinder.CustomVisionEndPoint
                };

                photoStream.Position = 0;
                var results = await endpoint.DetectImageAsync(
                    Guid.Parse(ApiKeysFinder.ProjectId),
                    ApiKeysFinder.PublishedName,
                    photoStream
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
