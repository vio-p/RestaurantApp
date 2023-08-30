using RestaurantApp.Commands;
using RestaurantApp.Stores;
using System.Windows.Input;

namespace RestaurantApp.ViewModels;

public class AdministratorViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public ICommand LogOutCommand { get; }
    public ICommand ShowWaitersPageCommand { get; }
    public ICommand ShowTablesPageCommand { get; }
    public ICommand ShowProductsPageCommand { get; }

    public AdministratorViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;

        LogOutCommand = new RelayCommand(LogOut);
        ShowWaitersPageCommand = new RelayCommand(ShowWaitersPage);
        ShowTablesPageCommand = new RelayCommand(ShowTablesPage);
        ShowProductsPageCommand = new RelayCommand(ShowProductsPage);
    }

    private ViewModelBase _currentPageViewModel;
    public ViewModelBase CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set
        {
            _currentPageViewModel = value;
            OnPropertyChanged(nameof(CurrentPageViewModel));
        }
    }

    private void LogOut()
    {
        _navigationStore.CurrentViewModel = new LogInViewModel(_navigationStore);
    }

    private void ShowWaitersPage()
    {
        CurrentPageViewModel = new AdministratorWaitersViewModel();
    }

    private void ShowTablesPage()
    {
        CurrentPageViewModel = new AdministratorTablesViewModel();
    }

    private void ShowProductsPage()
    {
        CurrentPageViewModel = new AdministratorProductsViewModel();
    }
}
