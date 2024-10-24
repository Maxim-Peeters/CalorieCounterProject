using Microsoft.AspNetCore.Mvc;
using CalorieCounterAPI.Services;
using System.Threading.Tasks;
using System.IO;
using CalorieCounterAPI.Services;
using CalorieCounter.Services;
using System.Diagnostics;
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

            var croppedImagePath = Path.Combine(Path.GetTempPath(), "cropped.png");
            Debug.WriteLine($"UploadImage: Processing image to get barcode from: {tempPath}");

            var detectedBarcode = await BarcodeFinderService.ProcessImageAndGetBarcodeAsync(tempPath, croppedImagePath);

            if (string.IsNullOrEmpty(detectedBarcode))
            {
                Debug.WriteLine("UploadImage: No barcode detected");
                return NotFound("No barcode detected");
            }

            Debug.WriteLine($"UploadImage: Detected barcode: {detectedBarcode}");

            var productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(detectedBarcode);
            if (productInfo == null)
            {
                Debug.WriteLine("UploadImage: No product information found for the barcode");
                return NotFound("No product information found for the barcode");
            }

            Debug.WriteLine("UploadImage: Product information retrieved successfully");
            return Ok(productInfo);
        }

    }
}
