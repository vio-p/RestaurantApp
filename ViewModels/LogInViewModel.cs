using RestaurantApp.Stores;
using RestaurantApp.Commands;
using System.Windows.Input;
using RestaurantApp.Models;
using System.Linq;
using System.Windows;

namespace RestaurantApp.ViewModels;

public class LogInViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public ICommand LogInCommand { get; }
    
    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            CanLogIn = !string.IsNullOrEmpty(_username);
            OnPropertyChanged(nameof(Username));
        }
    }

    public bool CanLogIn { get; set; } = false;

    public LogInViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;

        LogInCommand = new RelayCommand(LogIn, parameter => CanLogIn);
    }

    private void LogIn()
    {
        using (RestaurantContext context = new())
        {
            User? user = context.Users.SingleOrDefault(user => user.Username == Username && user.Active);
            if (user == null)
            {
                _ = MessageBox.Show("No user with this username was found!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (user is Administrator)
            {
                _navigationStore.CurrentViewModel = new AdministratorViewModel(_navigationStore);
                return;
            }
            if (user is Waiter)
            {
                _navigationStore.CurrentViewModel = new WaiterViewModel(_navigationStore);
                return;
            }
        }
    }
}
