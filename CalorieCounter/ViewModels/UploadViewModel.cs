using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CalorieCounter.Services;
using CalorieCounter.Helpers;
using System;

namespace CalorieCounter.ViewModels
{
    public class UploadViewModel : ObservableObject
    {
        private bool isRunning = false;
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value);
        }

        private ImageSource selectedImageSource;
        public ImageSource SelectedImageSource
        {
            get => selectedImageSource;
            set => SetProperty(ref selectedImageSource, value);
        }

        private string dailyCalories;
        public string DailyCalories
        {
            get => dailyCalories;
            set => SetProperty(ref dailyCalories, value);
        }

        public ICommand ChooseImageCommand { get; }
        public ICommand UploadImageCommand { get; }

        public UploadViewModel()
        {
            ChooseImageCommand = new AsyncRelayCommand(PickImageAsync);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
        }

        // Step 1: Pick the image from the gallery
        private async Task PickImageAsync()
        {
            // Open image picker to choose a photo
            var fileResult = await MediaPicker.Default.PickPhotoAsync();
            if (fileResult != null)
            {
                // Open the image as a stream for processing
                var photoStream = await fileResult.OpenReadAsync();

                // Bind the photo stream to the UI for display
                SelectedImageSource = ImageSource.FromStream(() => photoStream);
            }
        }

        // Step 2: Upload and process the image for barcode scanning
        private async Task UploadImageAsync()
        {
            if (SelectedImageSource != null)
            {
                IsRunning = true;

                try
                {
                    // Ensure we have a file result for the selected image
                    var fileResult = await MediaPicker.Default.PickPhotoAsync();
                    if (fileResult == null)
                    {
                        DailyCalories = "No image selected.";
                        return;
                    }

                    // Open the photo stream for further processing
                    var photoStream = await fileResult.OpenReadAsync();

                    // Step 1: Process image using BarcodeFinderService
                    var outputCroppedPath = Path.Combine(FileSystem.CacheDirectory, "croppedImage.png");
                    var isBarcodeFound = await BarcodeFinderService.ProcessImageAndGetBarcodeAsync(photoStream, outputCroppedPath);

                    if (isBarcodeFound)
                    {
                        // Step 2: Extract barcode number using BarcodeReaderService
                        var barcodeNumber = await BarcodeReaderService.AnalyzeImageOCRAsync(photoStream);
                        if (!string.IsNullOrEmpty(barcodeNumber))
                        {
                            // Step 3: Get product info from OpenFoodFactsService
                            var productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(barcodeNumber);

                            if (productInfo != null)
                            {
                                // Parse and display calories from the product
                                var parts = productInfo.Split(';');
                                DailyCalories = $"Product: {parts[0]}, Calories: {parts[1]} kcal";
                            }
                            else
                            {
                                DailyCalories = "Product not found or invalid barcode.";
                            }
                        }
                        else
                        {
                            DailyCalories = "No barcode detected.";
                        }
                    }
                    else
                    {
                        DailyCalories = "No barcode found in the image.";
                    }
                }
                catch (Exception ex)
                {
                    DailyCalories = $"Error: {ex.Message}";
                }
                finally
                {
                    IsRunning = false;
                }
            }
        }
    }
}
