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
        private Product currentProduct;

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

        private bool _isProductInfoAvailable;

        public bool IsProductInfoAvailable
        {
            get => _isProductInfoAvailable;
            set
            {
                _isProductInfoAvailable = value;
                OnPropertyChanged(nameof(IsProductInfoAvailable));
            }
        }


        // Commands
        public ICommand PickAndSearchBarcodeCommand { get; set; }
        public ICommand TakeAndSearchBarcodeCommand { get; set; }
        public ICommand UploadImageCommand { get; set; }
        public ICommand CalculateCaloriesCommand { get; set; }
        public ICommand OpenSummaryCommand { get; set; }
        public ICommand AddProductCommand { get; set; }

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
            AddProductCommand = new AsyncRelayCommand(AddProduct);
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
                var croppedImagePath = await BarcodeFinderService.ProcessImageAndGetBarcodeAsync(photo.FullPath);

                if (croppedImagePath != null)
                {
                    string barcodeNumber = await BarcodeReaderService.AnalyzeImageOCRAsync(croppedImagePath);
                    var productInfo = await OpenFoodFactsService.GetProductInfoByBarcode(barcodeNumber);

                    if (!string.IsNullOrEmpty(productInfo))
                    {
                        var parts = productInfo.Split(';');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int caloriesPer100gValue))
                        {
                            ProductName = parts[0];
                            CaloriesPer100g = parts[1];
                            Calories = $"Calories per 100g: {caloriesPer100gValue}";

                            var totalCalories = (caloriesPer100gValue * int.Parse(GramsEaten)) / 100;
                            CaloriesEaten = $"{totalCalories}";

                            currentProduct = new Product
                            {
                                Name = ProductName,
                                CaloriesPer100g = caloriesPer100gValue,
                                AmountInGrams = int.Parse(GramsEaten),
                                Barcode = barcodeNumber,
                                Category = SelectedCategory
                            };
                            IsProductInfoAvailable = true; // Set to true when data is populated

                            // Don't post the product yet, just store it.
                            Calories = "Product ready to add. Press 'Add Product'.";
                        }
                        else
                        {
                            Calories = "Product information is incomplete.";
                            IsProductInfoAvailable = false;

                        }
                    }
                    else
                    {
                        Calories = "Product not found or error retrieving data.";
                        IsProductInfoAvailable = false;

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

        private async Task AddProduct()
        {
            if (currentProduct == null)
            {
                Calories = "No product to add.";
                return;
            }

            try
            {
                await ApiService<Product>.PostAsync("Calories", currentProduct); // Post to API
                Calories = "Product added successfully.";
                await GoToSummary();
            }
            catch (Exception ex)
            {
                Calories = $"Error adding product: {ex.Message}";
            }
        }

        private async Task GoToSummary()
        {
            await _navigationService.NavigateToSummaryPageAsync();
        }
    }
}
