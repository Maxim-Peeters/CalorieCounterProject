using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace CalorieCounter.ViewModels
{
    public interface IUploadViewModel
    {
        // Commands
        ICommand PickAndSearchBarcodeCommand { get; set; }
        ICommand TakeAndSearchBarcodeCommand { get; set; }
        ICommand UploadImageCommand { get; set; }
        ICommand OpenSummaryCommand { get; set; }

        // Properties for UI bindings
        string Calories { get; set; }
        string GramsEaten { get; set; }
        string ProductName { get; set; }
        string CaloriesPer100g { get; set; }
        bool IsRunning { get; set; }
        ImageSource SelectedImageSource { get; set; }
    }
}
