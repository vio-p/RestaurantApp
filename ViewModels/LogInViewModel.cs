using RestaurantApp.Stores;

namespace RestaurantApp.ViewModels;

public class LogInViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public LogInViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
    }
}
