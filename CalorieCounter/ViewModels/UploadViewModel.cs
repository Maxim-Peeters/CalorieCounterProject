using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CalorieCounter.Services;
using System.Diagnostics;

namespace CalorieCounter.ViewModels
{
    public partial class UploadViewModel : ObservableObject
    {
        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private readonly ImageUploadService _imageUploadService;

        public UploadViewModel()
        {
            _imageUploadService = new ImageUploadService();
            ChooseImageCommand = new AsyncRelayCommand(ChooseImageAsync);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
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
                    Debug.WriteLine($"Selected file: {result.FullPath}");
                    ImageSource = result.FullPath;
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
                var (success, error) = await _imageUploadService.UploadImageAsync(ImageSource);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Image uploaded successfully!", "OK");
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