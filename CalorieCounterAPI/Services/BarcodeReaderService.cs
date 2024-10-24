using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Text.RegularExpressions;
// Manage NUGet Packages
// Microsoft.Azure.CognitiveServices.Vision.ComputerVision 7.0.0
namespace CalorieCounterAPI.Services
{
    public static class ApiKeys
    {
        public static string ComputerVisionEndPoint => "https://dv-computer-vision.cognitiveservices.azure.com/";
        public static string PredictionKey => "a9b387f8768a461f956bde1ae038d9b3";
    }
    public class BarcodeReaderService
    {
        private static async Task<ReadResult> ProcessFile(ComputerVisionClient
        client, string pathToFile)
        {
            FileStream stream = File.OpenRead(pathToFile);
            ReadInStreamHeaders textHeaders = await
            client.ReadInStreamAsync(stream);
            Thread.Sleep(2000);
            string operationLocation = textHeaders.OperationLocation;
            string operationId = operationLocation[^36..];
            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted));
            return results.AnalyzeResult.ReadResults.First();
        }
        public async static Task<ReadResult> AnalyzeImageOCR(string
        outputCroppedPath)
        {
            ComputerVisionClient client = new(new
            ApiKeyServiceClientCredentials(ApiKeys.PredictionKey))
            {
                Endpoint = ApiKeys.ComputerVisionEndPoint
            };
            return await ProcessFile(client, outputCroppedPath);
        }
    }
}

