using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieCounterAPI.Services
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

        private static async Task<ReadResult> ProcessFile(ComputerVisionClient client, string pathToFile)
        {
            try
            {
                using var stream = File.OpenRead(pathToFile);

                // Use the current SDK method signature
                var textHeaders = await client.ReadInStreamAsync(
                    stream,
                    language: "en"  // Optional: specify language if known
                );

                string operationLocation = textHeaders.OperationLocation;
                string operationId = operationLocation.Substring(operationLocation.Length - 36);

                ReadOperationResult results;
                do
                {
                    results = await client.GetReadResultAsync(Guid.Parse(operationId));
                    await Task.Delay(1000); // Use Task.Delay instead of Thread.Sleep
                } while (results.Status == OperationStatusCodes.Running ||
                        results.Status == OperationStatusCodes.NotStarted);

                return results.AnalyzeResult.ReadResults.FirstOrDefault();
            }
            catch (ComputerVisionOcrErrorException ex)
            {
                Debug.WriteLine($"Azure Vision Error: {ex.Response?.Content ?? ex.Message}");
                throw;
            }
        }

        public static async Task<ReadResult> AnalyzeImageOCR(string outputCroppedPath)
        {
            using var client = CreateAuthorizedClient();
            return await ProcessFile(client, outputCroppedPath);
        }
    }
}
