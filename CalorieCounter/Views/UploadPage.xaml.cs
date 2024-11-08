using Microsoft.Maui.Controls;
using CalorieCounter.ViewModels;

namespace CalorieCounter.Views
{
    public partial class UploadPage : ContentPage
    {
        public UploadPage(UploadViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;  // Use the passed-in viewModel instance
        }
    }
}
