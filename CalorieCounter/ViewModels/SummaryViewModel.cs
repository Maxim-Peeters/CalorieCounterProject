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

        public ICommand GoBackCommand { get; set; }
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
            GoBackCommand = new AsyncRelayCommand(GoBack);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var days = await _productDataService.GetDaysAsync();
                AllDays = new ObservableCollection<Day>(days);

                var today = days.FirstOrDefault(d => d.Date.Date == Date.Date);
                Products = new ObservableCollection<Product>(today?.Products ?? new List<Product>());

                // Log the product IDs to verify they are being fetched correctly
                

                TotalCalories = Products.Sum(p => p.TotalCalories);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                Products = new ObservableCollection<Product>();
                AllDays = new ObservableCollection<Day>();
                TotalCalories = 0;
                await App.Current.MainPage.DisplayAlert("Error", "Failed to load data. Please try again later.", "OK");
            }
        }

        public async Task GoBack()
        {
            await _navigationService.NavigateBackAsync();
        }

       


    }
}
