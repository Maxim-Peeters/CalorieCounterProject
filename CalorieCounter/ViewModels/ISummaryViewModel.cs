using CalorieCounter.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

public interface ISummaryViewModel
{
    DateTime Date { get; set; }
    ObservableCollection<Product> Products { get; set; }
    int TotalCalories { get; set; }
    ICommand GoBackCommand { get; set; }
    ICommand RefreshCommand { get; set; }
    ICommand EditCommand { get; set; }
    ICommand DeleteCommand { get; set; }

    Task LoadDataAsync();
    
}
