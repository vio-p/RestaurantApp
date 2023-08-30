using RestaurantApp.Commands;
using RestaurantApp.Models;
using RestaurantApp.Stores;
using System;
using System.Windows.Input;

namespace RestaurantApp.ViewModels;

public class WaiterViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly Waiter _loggedInWaiter;

    public ICommand LogOutCommand { get; }
    public ICommand ShowOrdersPageCommand { get; }

    public WaiterViewModel(NavigationStore navigationStore, Waiter loggedInWaiter)
    {
        _navigationStore = navigationStore;
        _loggedInWaiter = loggedInWaiter;

        LogOutCommand = new RelayCommand(LogOut);
        ShowOrdersPageCommand = new RelayCommand(ShowOrdersPage);
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

    private void ShowOrdersPage()
    {
        CurrentPageViewModel = new WaiterOrdersViewModel(_loggedInWaiter);
    }
}
