using CalorieCounter.Services;
using CalorieCounter.ViewModels;
using CalorieCounter.Views;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToSummaryPageAsync()
    {
        var productDataService = _serviceProvider.GetRequiredService<ProductDataService>();
        var summaryViewModel = new SummaryViewModel(productDataService, this);
        var summaryPage = new SummaryPage(summaryViewModel);
        await Application.Current.MainPage.Navigation.PushAsync(summaryPage);
    }

    public async Task NavigateBackAsync()
    {
        await Application.Current.MainPage.Navigation.PopAsync();
    }
}