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
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Please select an image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    string tempFilePath;

                    // For mobile platforms, read the file stream into a temporary file
                    if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        tempFilePath = Path.Combine(FileSystem.CacheDirectory, result.FileName);

                        using (var stream = await result.OpenReadAsync())
                        using (var tempFile = File.Create(tempFilePath))
                        {
                            await stream.CopyToAsync(tempFile);
                        }
                    }
                    else
                    {
                        // For Windows, use the file path directly
                        tempFilePath = result.FullPath;
                    }

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
