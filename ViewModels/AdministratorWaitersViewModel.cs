using Microsoft.EntityFrameworkCore;
using RestaurantApp.Commands;
using RestaurantApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RestaurantApp.ViewModels;

public class AdministratorWaitersViewModel : ViewModelBase
{
    public ObservableCollection<Waiter> Waiters { get; }

    public ICommand AddWaiterCommand { get; }
    public ICommand ModifyWaiterCommand { get; }
    public ICommand DeleteWaiterCommand { get; }

    public AdministratorWaitersViewModel()
    {
        using RestaurantContext context = new();
        Waiters = new(context.Users.OfType<Waiter>().Where(waiter => waiter.Active).ToList());

        AddWaiterCommand = new RelayCommand(AddWaiter, parameter => InputIsValid());
        ModifyWaiterCommand = new RelayCommand(ModifyWaiter, parameter => InputIsValid() && SelectedWaiter != null);
        DeleteWaiterCommand = new RelayCommand(DeleteWaiter, parameter => SelectedWaiter != null);
    }

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
        }
    }

    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    private Waiter _selectedWaiter;
    public Waiter SelectedWaiter
    {
        get => _selectedWaiter;
        set
        {
            _selectedWaiter = value;
            if (_selectedWaiter != null)
            {
                FirstName = _selectedWaiter.FirstName;
                LastName = _selectedWaiter.LastName;
                Username = _selectedWaiter.Username;
            }
            else
            {
                FirstName = null!;
                LastName = null!;
                Username = null!;
            }
            OnPropertyChanged(nameof(SelectedWaiter));
        }
    }

    private void AddWaiter()
    {
        using RestaurantContext context = new();
        if (context.Users.OfType<Waiter>().SingleOrDefault(user => user.Username == Username && user.Active) != null)
        {
            _ = MessageBox.Show("There is already a user with this username!", "Invalid username", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Waiter waiter = new()
        {
            Username = Username,
            FirstName = FirstName,
            LastName = LastName
        };
        Waiters.Add(waiter);

        context.Users.Add(waiter);
        context.SaveChanges();
    }

    private void ModifyWaiter()
    {
        using RestaurantContext context = new();
        if (context.Users.OfType<Waiter>().SingleOrDefault(waiter => waiter.Id != SelectedWaiter.Id && waiter.Username == Username && waiter.Active) != null)
        {
            _ = MessageBox.Show("There is already a user with this username!", "Invalid username", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedWaiter.Username = Username;
        SelectedWaiter.FirstName = FirstName;
        SelectedWaiter.LastName = LastName;

        Waiter dbWaiter = context.Users.OfType<Waiter>().Single(waiter => waiter.Id == SelectedWaiter.Id);
        dbWaiter.Username = Username;
        dbWaiter.FirstName = FirstName;
        dbWaiter.LastName = LastName;
        context.SaveChanges();
    }

    private void DeleteWaiter()
    {
        using RestaurantContext context = new();
        Waiter dbWaiter = context.Users.OfType<Waiter>().Single(waiter => waiter.Id == SelectedWaiter.Id);
        dbWaiter.Active = false;

        foreach (Table table in context.Tables)
        {
            if (table.WaiterId ==  SelectedWaiter.Id)
            {
                table.Waiter = null;
            }
        }

        context.SaveChanges();

        Waiters.Remove(SelectedWaiter);
    }

    private bool InputIsValid()
    {
        return !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(Username);
    }
}
