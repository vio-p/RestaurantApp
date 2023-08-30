using Microsoft.EntityFrameworkCore;
using RestaurantApp.Commands;
using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace RestaurantApp.ViewModels;

public class WaiterOrdersViewModel : ViewModelBase
{
    private readonly Waiter _loggedInWaiter;

    public List<OrderState> OrderStates { get; } = Enum.GetValues<OrderState>().ToList();

    public ObservableCollection<Table> AvailableTables { get; }
    public ObservableCollection<Order> Orders { get; }

    public ICommand StartOrderCommand { get; }

    public WaiterOrdersViewModel(Waiter loggedInWaiter)
    {
        _loggedInWaiter = loggedInWaiter;

        using RestaurantContext context = new();
        AvailableTables = new(context.Tables.Where(table => table.Active && table.Available));
        Orders = new(context.Orders.Include("Table"));

        StartOrderCommand = new RelayCommand(AddOrder);
    }

    private Table _selectedTable;
    public Table SelectedTable
    {
        get => _selectedTable;
        set
        {
            _selectedTable = value;
            OnPropertyChanged(nameof(SelectedTable));
        }
    }

    private string _occupiedSeats;
    public string OccupiedSeats
    {
        get => _occupiedSeats;
        set
        {
            _occupiedSeats = value;
            OnPropertyChanged(nameof(OccupiedSeats));
        }
    }

    private void AddOrder()
    {
        using RestaurantContext context = new();

        Order order = new()
        {
            Date = DateTime.Now,
            Table = context.Tables.Single(table => table.Id == SelectedTable.Id),
            Waiter = context.Users.OfType<Waiter>().Single(waiter => waiter.Id == _loggedInWaiter.Id),
            OccupiedSeats = int.Parse(OccupiedSeats)
        };
        Orders.Add(order);

        // set table to be unavailable in database, remove from Tables
        // data validation, don't forget

        context.Orders.Add(order);
        context.SaveChanges();
    }

    private bool InputIsValid()
    {
        return true;
    }
}
