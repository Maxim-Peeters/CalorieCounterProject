using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            public static async Task<string> ProcessImageAndGetBarcodeAsync(string inputImagePath, string outputCroppedPath)
            {
                try
                {
                    // Read the image file
                    using (var fileStream = new FileStream(inputImagePath, FileMode.Open))
                    {
                        // Detect objects in the image
                        var prediction = await DetectObjectAsync(fileStream);

                        if (prediction != null && prediction.Probability > MinProbability)
                        {
                            // Use ImageHelper to crop the image
                            ImageHelper.CropBoundingBox(inputImagePath, prediction.BoundingBox, outputCroppedPath);

                            // Here you would add barcode reading logic
                            // For now, we'll return a dummy barcode based on the prediction
                            return GenerateDummyBarcode(prediction);
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing image: {ex.Message}");
                    throw;
                }
                finally
                {
                    // Clean up the input file if it exists
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
                    var endpoint = new CustomVisionPredictionClient(
                        new ApiKeyServiceClientCredentials(ApiKeysFinder.PredictionKey))
                    {
                        Endpoint = ApiKeysFinder.CustomVisionEndPoint
                    };

                    photoStream.Position = 0;
                    var results = await endpoint.DetectImageAsync(
                        Guid.Parse(ApiKeysFinder.ProjectId),
                        ApiKeysFinder.PublishedName,
                        photoStream
                    );

                    return results.Predictions
                        .OrderByDescending(x => x.Probability)
                        .FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error detecting object: {ex.Message}");
                    throw;
                }
            }

            private static string GenerateDummyBarcode(PredictionModel prediction)
            {
                // This is a placeholder method - in a real implementation,
                // you would use a barcode reading library to extract the actual barcode
                // from the cropped image
                return $"123456789{(int)(prediction.Probability * 100)}";
            }
        }
    }

    
