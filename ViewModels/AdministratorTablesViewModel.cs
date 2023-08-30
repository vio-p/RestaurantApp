using RestaurantApp.Models;
using RestaurantApp.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Windows;
using System.IO.Packaging;

namespace RestaurantApp.ViewModels;

public class AdministratorTablesViewModel : ViewModelBase
{
    public ObservableCollection<Table> Tables { get; }
    public ObservableCollection<Waiter> Waiters { get; }

    public ICommand AddTableCommand { get; }
    public ICommand ModifyTableCommand { get; }
    public ICommand DeleteTableCommand { get; }

    public AdministratorTablesViewModel()
    {
        using RestaurantContext context = new();
        Tables = new(context.Tables.Where(table => table.Active).ToList());
        Waiters = new(context.Users.OfType<Waiter>().Where(waiter => waiter.Active).ToList());

        AddTableCommand = new RelayCommand(AddTable, parameter => InputIsValid());
        ModifyTableCommand = new RelayCommand(ModifyTable, parameter => InputIsValid() && SelectedTable != null);
        DeleteTableCommand = new RelayCommand(DeleteTable, parameter => SelectedTable != null);
    }

    private string _number;
    public string Number
    {
        get => _number;
        set
        {
            _number = value;
            OnPropertyChanged(nameof(Number));
        }
    }

    private string _availableSeats;
    public string AvailableSeats
    {
        get => _availableSeats;
        set
        {
            _availableSeats = value;
            OnPropertyChanged(nameof(AvailableSeats));
        }
    }

    private Waiter _assignedWaiter;
    public Waiter AssignedWaiter
    {
        get => _assignedWaiter;
        set
        {
            _assignedWaiter = value;
            OnPropertyChanged(nameof(AssignedWaiter));
        }
    }

    private Table _selectedTable;
    public Table SelectedTable
    {
        get => _selectedTable;
        set
        {
            _selectedTable = value;
            if (_selectedTable != null)
            {
                Number = _selectedTable.Number.ToString();
                AvailableSeats = _selectedTable.AvailableSeats.ToString();
                if (_selectedTable.WaiterId != null)
                {
                    AssignedWaiter = Waiters.Single(waiter => waiter.Id == _selectedTable.WaiterId);
                }  
            }
            else
            {
                Number = null!;
                AvailableSeats = null!;
                AssignedWaiter = null!;
            }
            OnPropertyChanged(nameof(SelectedTable));
        }
    }

    private void AddTable()
    {
        using RestaurantContext context = new();
        if (context.Tables.SingleOrDefault(table => table.Number == int.Parse(Number) && table.Active) != null)
        {
            _ = MessageBox.Show("There is already a table with this number!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Table table = new()
        {
            AvailableSeats = int.Parse(AvailableSeats),
            Number = int.Parse(Number),
            Waiter = context.Users.OfType<Waiter>().Single(waiter => waiter.Id == AssignedWaiter.Id)
        };
        Tables.Add(table);

        context.Tables.Add(table);
        context.SaveChanges();
    }

    private void ModifyTable()
    {
        using RestaurantContext context = new();
        if (context.Tables.SingleOrDefault(table => table.Id != SelectedTable.Id && table.Number == int.Parse(Number) && table.Active) != null)
        {
            _ = MessageBox.Show("There is already a table with this number!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedTable.Number = int.Parse(Number);
        SelectedTable.AvailableSeats = int.Parse(AvailableSeats);
        SelectedTable.WaiterId = AssignedWaiter.Id;
        SelectedTable.Waiter = Waiters.Single(waiter => waiter.Id == AssignedWaiter.Id);

        Table dbTable = context.Tables.Single(table => table.Id == SelectedTable.Id);
        dbTable.Number = SelectedTable.Number;
        dbTable.AvailableSeats = SelectedTable.AvailableSeats;
        dbTable.WaiterId = AssignedWaiter.Id;
        context.SaveChanges();
    }

    private void DeleteTable()
    {
        using RestaurantContext context = new();
        Table dbTable = context.Tables.Single(table => table.Id == SelectedTable.Id);
        dbTable.Active = false;
        context.SaveChanges();

        Tables.Remove(SelectedTable);
    }

    private bool InputIsValid()
    {
        if (string.IsNullOrEmpty(Number) || string.IsNullOrEmpty(AvailableSeats) || AssignedWaiter == null)
        {
            return false;
        }
        bool numberIsValid = int.TryParse(Number, out _);
        bool availableSeatsIsValid = int.TryParse(AvailableSeats, out _);
        return numberIsValid && availableSeatsIsValid;
    }
}
