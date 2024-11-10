using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using CalorieCounter.Services;
using System;
using System.Windows.Input;
using CalorieCounter.Models;

namespace CalorieCounter.ViewModels
{
    public class UploadViewModel : ObservableObject, IUploadViewModel
    {
        private bool isRunning = false;
        private INavigationService _navigationService;

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

        private string calories = "No image processed yet";

        public string Calories
        {
            get => calories;
            set => SetProperty(ref calories, value);
        }

        private string gramsEaten;

        public string GramsEaten
        {
            get => gramsEaten;
            set => SetProperty(ref gramsEaten, value);
        }

        private string productName;

        public string ProductName
        {
            get => productName;
            set => SetProperty(ref productName, value);
        }

        private string caloriesPer100g;

        public string CaloriesPer100g
        {
            get => caloriesPer100g;
            set => SetProperty(ref caloriesPer100g, value);
        }

        private string caloriesEaten;

        public string CaloriesEaten
        {
            get => caloriesEaten;
            set => SetProperty(ref caloriesEaten, value);
        }
        private Category selectedCategory;

        public Category SelectedCategory
        {
            get => selectedCategory;
            set => SetProperty(ref selectedCategory, value);
        }

        // Commands
        public ICommand PickAndSearchBarcodeCommand { get; set; }
        public ICommand TakeAndSearchBarcodeCommand { get; set; }
        public ICommand UploadImageCommand { get; set; }
        public ICommand CalculateCaloriesCommand { get; set; }
        public ICommand OpenSummaryCommand { get; set; }

        public UploadViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            BindCommands();
        }

        private void BindCommands()
        {

            PickAndSearchBarcodeCommand = new AsyncRelayCommand(PickAndSearchBarcode);
            TakeAndSearchBarcodeCommand = new AsyncRelayCommand(TakeAndSearchBarcode);
            UploadImageCommand = new AsyncRelayCommand(UploadImage);
            OpenSummaryCommand = new AsyncRelayCommand(GoToSummary);

        }

        private async Task PickAndSearchBarcode()
        {
            if (IsRunning)
                return;

            try
            {
                // Ask the user to pick a photo
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    // Show the photo preview
                    SelectedImageSource = ImageSource.FromFile(photo.FullPath);

                    // Ask for the grams eaten via a popup
                    var gramsResult = await Application.Current.MainPage.DisplayPromptAsync("Enter Grams Eaten", "How many grams did you eat?", initialValue: "", keyboard: Keyboard.Numeric);

                    if (gramsResult != null && int.TryParse(gramsResult, out int grams))
                    {
                        GramsEaten = gramsResult;

                        // Now show the category selection popup
                        var categoryResult = await Application.Current.MainPage.DisplayActionSheet("Select Category", "Cancel", null, Enum.GetNames(typeof(Category)).ToArray());

                        if (categoryResult != null && categoryResult != "Cancel")
                        {
                            // Log category result for debugging
                            Console.WriteLine($"Selected category: {categoryResult}");

                            // Try to parse the selected category
                            if (Enum.TryParse(categoryResult, out Category parsedCategory))
                            {
                                SelectedCategory = parsedCategory;
                                Calories = $"Category: {SelectedCategory}";
                            }
                            else
                            {
                                Calories = "Invalid category selected.";
                            }

                            await ProcessImage(photo);
                        }
                        else
                        {
                            Calories = "Please select a valid category.";
                        }
                    }
                    else
                    {
                        Calories = "Please enter a valid number of grams.";
                    }
                }
            }
            catch (Exception ex)
            {
                Calories = $"Error picking photo: {ex.Message}";
            }
        }

        private async Task TakeAndSearchBarcode()
        {
            if (IsRunning)
                return;

            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Capture the photo
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        // Show the photo preview
                        SelectedImageSource = ImageSource.FromFile(photo.FullPath);

                        // Ask for the grams eaten via a popup
                        var gramsResult = await Application.Current.MainPage.DisplayPromptAsync("Enter Grams Eaten", "How many grams did you eat?", initialValue: "", keyboard: Keyboard.Numeric);

                        if (gramsResult != null && int.TryParse(gramsResult, out int grams))
                        {
                            GramsEaten = gramsResult;

                            // Now show the category selection popup
                            var categoryResult = await Application.Current.MainPage.DisplayActionSheet("Select Category", "Cancel", null, Enum.GetNames(typeof(Category)).ToArray());

                            if (categoryResult != null && categoryResult != "Cancel")
                            {
                                // Log category result for debugging
                                Console.WriteLine($"Selected category: {categoryResult}");

                                // Try to parse the selected category
                                if (Enum.TryParse(categoryResult, out Category parsedCategory))
                                {
                                    SelectedCategory = parsedCategory;
                                    Calories = $"Category: {SelectedCategory}";
                                }
                                else
                                {
                                    Calories = "Invalid category selected.";
                                }

                                // Process the captured image
                                await ProcessImage(photo);
                            }
                            else
                            {
                                Calories = "Please select a valid category.";
                            }
                        }
                        else
                        {
                            Calories = "Please enter a valid number of grams.";
                        }
                    }
                }
                else
                {
                    Calories = "Camera capture is not supported on this device.";
                }
            }
            catch (Exception ex)
            {
                Calories = $"Error capturing photo: {ex.Message}";
            }
        }

        private async Task UploadImage()
        {
            if (IsRunning)
                return;

            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    await ProcessImage(photo);
                }
            }
            catch (Exception ex)
            {
                Calories = $"Error uploading image: {ex.Message}";
            }
        }

        private async Task ProcessImage(FileResult photo)
        {
            IsRunning = true;
            Calories = "Processing image...";

            try
            {
                // Process the image and find barcode
                var croppedImagePath = await BarcodeFinderService.ProcessImageAndGetBarcodeAsync(photo.FullPath);

                if (croppedImagePath != null)
                {
                    // Perform OCR to get the barcode number
                    string barcodeNumber = await BarcodeReaderService.AnalyzeImageOCRAsync(croppedImagePath);

                    // Get product info from OpenFoodFacts API using barcode
                    var productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(barcodeNumber);

                    if (!string.IsNullOrEmpty(productInfo))
                    {
                        // Split product info to extract name and calories per 100g
                        var parts = productInfo.Split(';');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int caloriesPer100gValue))
                        {
                            ProductName = parts[0];  // Product name
                            CaloriesPer100g = parts[1];  // Calories per 100g
                            Calories = $"Calories per 100g: {caloriesPer100gValue}";

                            // Now calculate the calories based on grams eaten
                            var totalCalories = (caloriesPer100gValue * int.Parse(GramsEaten)) / 100;
                            CaloriesEaten = $"{totalCalories}"; // Set the CaloriesEaten

                            // Prepare the product object
                            var product = new Product
                            {
                                Name = ProductName,
                                CaloriesPer100g = caloriesPer100gValue,
                                AmountInGrams = int.Parse(GramsEaten),
                                Barcode = barcodeNumber,
                                Category = SelectedCategory // You can categorize the product if needed, e.g., food category
                            };

                            // Send product data to the API
                            await ApiService<Product>.PostAsync("Calories", product);  // Assuming the endpoint is "Calories"
                        }
                        else
                        {
                            Calories = "Product information is incomplete.";
                        }
                    }
                    else
                    {
                        Calories = "Product not found or error retrieving data.";
                    }
                }
                else
                {
                    Calories = "No barcode detected.";
                }
            }
            catch (IOException ioEx)
            {
                Calories = $"Error reading file: {ioEx.Message}";
            }
            catch (Exception ex)
            {
                Calories = $"Error processing image: {ex.Message}";
            }
            finally
            {
                IsRunning = false;
            }
        }
        private async Task GoToSummary()
        {
            await _navigationService.NavigateToSummaryPageAsync();
        }
    }
}
