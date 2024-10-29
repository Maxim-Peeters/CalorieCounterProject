using Microsoft.AspNetCore.Mvc;
using CalorieCounterAPI.Services;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using CalorieCounter.Services;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models; // Ensure this is added

namespace CalorieCounterAPI.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Debug.WriteLine("UploadImage: No file uploaded");
                return BadRequest("No file uploaded");
            }

            string tempPath = Path.GetTempFileName();
            Debug.WriteLine($"UploadImage: Saving file to temporary path: {tempPath}");

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string croppedImagePath = Path.Combine(Path.GetTempPath(), "cropped.png");
            Debug.WriteLine($"UploadImage: Processing image to get barcode region from: {tempPath}");

            // Step 1: Detect and crop barcode
            bool cropSuccess = await BarcodeFinderService.ProcessImageAndGetBarcodeAsync(tempPath, croppedImagePath);

            if (!cropSuccess)
            {
                Debug.WriteLine("UploadImage: No barcode detected or failed to crop");
                return NotFound("No barcode detected or failed to crop");
            }

            // Step 2: Read the barcode from the cropped image
            var readResult = await BarcodeReaderService.AnalyzeImageOCR(croppedImagePath);

            if (readResult == null)
            {
                Debug.WriteLine("UploadImage: No readable barcode text found");
                return NotFound("No readable barcode text found");
            }

            // Extract the detected text
            string detectedBarcode = ExtractBarcodeText(readResult);

            if (string.IsNullOrEmpty(detectedBarcode))
            {
                Debug.WriteLine("UploadImage: No valid barcode text found");
                return NotFound("No valid barcode text found");
            }

            Debug.WriteLine($"UploadImage: Detected barcode text: {detectedBarcode}");

            var productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(detectedBarcode);
            if (productInfo == null)
            {
                Debug.WriteLine("UploadImage: No product information found for the barcode");
                return NotFound("No product information found for the barcode");
            }

            Debug.WriteLine("UploadImage: Product information retrieved successfully");
            return Ok(productInfo);
        }

        // Helper method to extract readable text from OCR result
        private string ExtractBarcodeText(ReadResult readResult)
        {
            return string.Join("", readResult.Lines.SelectMany(line => line.Words).Select(word => word.Text));
        }
    }
}
