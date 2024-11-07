using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CalorieCounter.Services; // Added for barcode and OpenFoodFacts services
using CalorieCounterAPI.Services;

namespace CalorieCounter.ViewModels
{
    public class UploadViewModel : INotifyPropertyChanged
    {
        private ImageSource _imageSource;
        private double _dailyCalories;
        private readonly HttpClient _httpClient;

        public UploadViewModel()
        {
            _httpClient = new HttpClient();
            ChooseImageCommand = new Command(async () => await ChooseImageAsync());
            UploadImageCommand = new Command(async () => await UploadImageAsync());
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        public double DailyCalories
        {
            get => _dailyCalories;
            set
            {
                _dailyCalories = value;
                OnPropertyChanged(nameof(DailyCalories));
            }
        }

        public ICommand ChooseImageCommand { get; }
        public ICommand UploadImageCommand { get; }

        private async Task ChooseImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an Image"
                });

                if (result != null)
                {
                    // Open the stream for the selected image
                    var stream = await result.OpenReadAsync();
                    // Set the image source to display the selected image
                    ImageSource = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image selection failed: {ex.Message}");
            }
        }

        private async Task UploadImageAsync()
        {
            try
            {
                // Get the stream from the selected image
                var stream = ((StreamImageSource)ImageSource)?.Stream?.Invoke(new System.Threading.CancellationToken()).Result;
                if (stream == null)
                {
                    Console.WriteLine("No image selected for upload.");
                    return;
                }

                // Process the image to extract barcode (this will crop and use OCR to read text)
                string barcode = await BarcodeReaderService.AnalyzeImageOCRAsync(stream);

                if (!string.IsNullOrEmpty(barcode))
                {
                    Console.WriteLine($"Barcode detected: {barcode}");

                    // Now fetch product information from OpenFoodFacts API
                    string productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(barcode);
                    if (!string.IsNullOrEmpty(productInfo))
                    {
                        var productDetails = productInfo.Split(';');
                        var calories = Convert.ToDouble(productDetails[1]);
                        DailyCalories = calories;  // Set the received calorie count
                    }
                    else
                    {
                        Console.WriteLine("Product information not found.");
                    }
                }
                else
                {
                    Console.WriteLine("No barcode detected in the image.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image upload or analysis failed: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
