using CalorieCounter.ViewModels;
using Microsoft.Maui.Controls;

namespace CalorieCounter.Views
{
    public partial class SummaryPage : ContentPage
    {
        public SummaryPage(ISummaryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        
    }
}
