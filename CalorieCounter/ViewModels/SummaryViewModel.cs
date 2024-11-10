using CalorieCounter.Models;
using CalorieCounter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalorieCounter.ViewModels
{
    public class SummaryViewModel : ObservableObject, ISummaryViewModel
    {
        private readonly ProductDataService _productDataService;
        private readonly INavigationService _navigationService;

        private DateTime date;
        private ObservableCollection<Product> products;
        private ObservableCollection<Day> allDays;
        private int totalCalories;

        public DateTime Date
        {
            get => date;
            set
            {
                if (SetProperty(ref date, value))
                {
                    Task.Run(() => LoadDataAsync());
                }
            }
        }

        public ObservableCollection<Product> Products
        {
            get => products;
            set => SetProperty(ref products, value);
        }

        public ObservableCollection<Day> AllDays
        {
            get => allDays;
            set => SetProperty(ref allDays, value);
        }

        public int TotalCalories
        {
            get => totalCalories;
            set => SetProperty(ref totalCalories, value);
        }

        public ICommand AddProductCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public SummaryViewModel(ProductDataService productDataService, INavigationService navigationService)
        {
            _productDataService = productDataService;
            _navigationService = navigationService;
            Date = DateTime.Now;
            Products = new ObservableCollection<Product>();
            AllDays = new ObservableCollection<Day>();
            LoadDataAsync();
            BindCommands();
        }

        private void BindCommands()
        {
            AddProductCommand = new AsyncRelayCommand(AddProduct);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            EditCommand = new AsyncRelayCommand<Product>(EditProduct);
            DeleteCommand = new AsyncRelayCommand<Product>(DeleteProduct);
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var days = await _productDataService.GetDaysAsync();
                AllDays = new ObservableCollection<Day>(days);

                var today = days.FirstOrDefault(d => d.Date.Date == Date.Date);
                Products = new ObservableCollection<Product>(today?.Products ?? new List<Product>());

                TotalCalories = Products.Sum(p => p.TotalCalories);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                Products = new ObservableCollection<Product>();
                AllDays = new ObservableCollection<Day>();
                TotalCalories = 0;
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load data. {ex.Message} Please try again later.", "OK");
            }
        }

        public async Task AddProduct()
        {
            await _navigationService.NavigateToUploadPageAsync();
        }

        public async Task EditProduct(Product product)
        {
            if (product == null)
            {
                return;
            }

            // Prompt for grams
            var gramsResponse = await App.Current.MainPage.DisplayPromptAsync(
                "Edit Grams",
                "Enter the new grams value:",
                initialValue: product.AmountInGrams.ToString(),
                maxLength: 10,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrEmpty(gramsResponse) || !int.TryParse(gramsResponse, out int newGrams))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid grams value.", "OK");
                return;
            }

            // Use DisplayActionSheet for category selection, like in UploadViewModel
            var categoryResponse = await App.Current.MainPage.DisplayActionSheet(
                "Select Category",
                "Cancel",
                null,
                Enum.GetNames(typeof(Category)).ToArray());

            if (categoryResponse == null || categoryResponse == "Cancel")
            {
                await App.Current.MainPage.DisplayAlert("Error", "Category selection was cancelled.", "OK");
                return;
            }

            // Parse the selected category
            if (!Enum.TryParse(categoryResponse, out Category newCategory))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid category selected.", "OK");
                return;
            }

            // Update the product details
            product.AmountInGrams = newGrams;
            product.Category = newCategory;

            try
            {
                // Call the API to update the product
                await ApiService<Product>.PutAsync($"Calories/{product.ProductId}", product);

                // Refresh the product list to reflect the changes
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error editing product: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "Failed to update the product. Please try again.", "OK");
            }
        }


        public async Task DeleteProduct(Product product)
        {
            if (product == null)
            {
                return;
            }

            // Show confirmation dialog
            var confirmed = await App.Current.MainPage.DisplayAlert(
                "Confirm Deletion",
                "Are you sure you want to delete this product?",
                "Yes",
                "No");

            if (!confirmed)
            {
                return; // User cancelled the deletion
            }

            try
            {
                await ApiService<object>.DeleteAsync($"Calories/{product.ProductId}");
                Products.Remove(product);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "Failed to delete the product. Please try again.", "OK");
            }
        }

    }
}