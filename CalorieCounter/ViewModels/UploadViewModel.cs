using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CalorieCounter.Services;
using System.Diagnostics;
using System.IO;

namespace CalorieCounter.ViewModels
{
    public partial class UploadViewModel : ObservableObject
    {
        private string _imageSource;
        private double _dailyCalories;
        private readonly ImageUploadService _imageUploadService;

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public double DailyCalories
        {
            get => _dailyCalories;
            set => SetProperty(ref _dailyCalories, value);
        }

        public UploadViewModel()
        {
            _imageUploadService = new ImageUploadService();
            ChooseImageCommand = new AsyncRelayCommand(ChooseImageAsync);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
            DailyCalories = 0; // Initialize daily calorie count
        }

        public IAsyncRelayCommand ChooseImageCommand { get; }
        public IAsyncRelayCommand UploadImageCommand { get; }

        private async Task ChooseImageAsync()
        {
            try
            {
                // Prompt the user to choose between taking a photo or selecting from the gallery
                string action = await Application.Current.MainPage.DisplayActionSheet(
                    "Choose Image Source",
                    "Cancel",
                    null,
                    "Take Photo",
                    "Choose from Gallery"
                );

                if (action == "Cancel")
                {
                    return; // Exit if user cancels
                }

                string tempFilePath = null;

                if (action == "Take Photo")
                {
                    // Use MediaPicker to take a new photo (supported on both mobile and Windows)
                    var photoResult = await MediaPicker.CapturePhotoAsync();
                    if (photoResult != null)
                    {
                        tempFilePath = await SaveToTempFileAsync(photoResult);
                    }
                }
                else if (action == "Choose from Gallery")
                {
                    // Use FilePicker to choose an image from the gallery
                    var result = await FilePicker.PickAsync(new PickOptions
                    {
                        PickerTitle = "Please select an image",
                        FileTypes = FilePickerFileType.Images
                    });

                    if (result != null)
                    {
                        if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
                        {
                            // Save to cache directory on mobile platforms
                            tempFilePath = await SaveToTempFileAsync(result);
                        }
                        else
                        {
                            // Use the file path directly on Windows
                            tempFilePath = result.FullPath;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tempFilePath))
                {
                    Debug.WriteLine($"Selected file path: {tempFilePath}");
                    ImageSource = tempFilePath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error choosing image: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to select image: " + ex.Message, "OK");
            }
        }

        private async Task<string> SaveToTempFileAsync(FileResult fileResult)
        {
            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileResult.FileName);

            using (var stream = await fileResult.OpenReadAsync())
            using (var tempFile = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(tempFile);
            }

            return tempFilePath;
        }


        private async Task UploadImageAsync()
        {
            if (string.IsNullOrEmpty(ImageSource))
            {
                await Application.Current.MainPage.DisplayAlert("Warning", "Please select an image to upload.", "OK");
                return;
            }

            try
            {
                Debug.WriteLine($"UploadImageAsync: Uploading image from path: {ImageSource}");
                var (success, error, body) = await _imageUploadService.UploadImageAsync(ImageSource);

                if (success)
                {
                    // Parse the response
                    string[] parts = body.Split(';');
                    string productName = parts[0];
                    double calories = double.Parse(parts[1]);

                    // Prompt for grams
                    var gramsInput = await Application.Current.MainPage.DisplayPromptAsync(
                        $"{productName} | {calories} kcal per 100g ",
                        "Enter Grams",
                        "Add",
                        initialValue: "100", // Default value
                        maxLength: 5,
                        keyboard: Keyboard.Numeric
                    );

                    if (double.TryParse(gramsInput, out double grams) && grams > 0)
                    {
                        DailyCalories += calories * (grams / 100);
                        await Application.Current.MainPage.DisplayAlert("Success", $"Calories added: {calories * (grams / 100)}", "OK");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Warning", "Please enter a valid amount in grams.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Upload failed: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadImageAsync: Error in UploadImageAsync: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred: " + ex.Message, "OK");
            }
        }
    }
}
