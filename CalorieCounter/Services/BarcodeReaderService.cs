using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieCounter.Services
{
    public static class ApiKeys
    {
        public static string ComputerVisionEndPoint => "https://mp-computervision.cognitiveservices.azure.com/";
        public static string PredictionKey => "1296ba6617084597a74c1b8969c92d5a";
    }

    public class BarcodeReaderService
    {
        private static ComputerVisionClient CreateAuthorizedClient()
        {
            var credentials = new ApiKeyServiceClientCredentials(ApiKeys.PredictionKey);
            var client = new ComputerVisionClient(credentials)
            {
                Endpoint = ApiKeys.ComputerVisionEndPoint
            };

            return client;
        }

        // Modify this method to process the file from a given path
        private static async Task<ReadResult?> ProcessFileAsync(ComputerVisionClient client, string imagePath)
        {
            try
            {
                // Open the file at the given path and process it
                using (var imageStream = File.OpenRead(imagePath))
                {
                    // Start the OCR read operation on the image stream
                    var textHeaders = await client.ReadInStreamAsync(
                        imageStream,
                        language: "en"  // Optional: specify language if known
                    );

                    string operationLocation = textHeaders.OperationLocation;
                    string operationId = operationLocation.Substring(operationLocation.Length - 36);

                    ReadOperationResult results;
                    do
                    {
                        results = await client.GetReadResultAsync(Guid.Parse(operationId));
                        await Task.Delay(1000); // Wait for OCR operation to complete
                    } while (results.Status == OperationStatusCodes.Running ||
                             results.Status == OperationStatusCodes.NotStarted);

                    // Return the first result if it exists
                    return results.AnalyzeResult?.ReadResults.FirstOrDefault();
                }
            }
            catch (ComputerVisionOcrErrorException ex)
            {
                Debug.WriteLine($"Azure Vision Error: {ex.Response?.Content ?? ex.Message}");
                throw;
            }
        }

        // Modify this method to accept a file path instead of a stream
        public static async Task<string> AnalyzeImageOCRAsync(string imagePath)
        {
            using var client = CreateAuthorizedClient();
            var readResult = await ProcessFileAsync(client, imagePath);

            if (readResult != null)
            {
                // Extract lines of text and concatenate them
                var barcodeText = string.Join(" ",
                    readResult.Lines.Select(line => line.Text));

                // Post-process the text to extract only numeric barcode information
                var barcodeNumber = new string(barcodeText.Where(char.IsDigit).ToArray());

                return barcodeNumber;
            }

            return string.Empty; // Return empty if no readable text found
        }
    }
}
