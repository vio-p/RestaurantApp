using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RestaurantApp.ViewModels;

class WaiterOrderHistoryViewModel : ViewModelBase
{
    private readonly Waiter _loggedInWaiter;

    public ObservableCollection<Order> Orders { get; }

    public WaiterOrderHistoryViewModel(Waiter loggedInWaiter)
    {
        _loggedInWaiter = loggedInWaiter;

        using RestaurantContext context = new();
        Orders = new(context.Orders.Where(order => order.WaiterId == _loggedInWaiter.Id && order.Date.Date == SelectedDate.Date && order.State != OrderState.Ongoing).Include("Table"));
        Total = Orders.Sum(order => order.Total);
    }

    private decimal _total = 0;
    public decimal Total
    {
        get => _total;
        set
        {
            _total = value;
            OnPropertyChanged(nameof(Total));
        }
    }

    private DateTime _selectedDate = DateTime.Now;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            Orders.Clear();
            using RestaurantContext context = new();
            context.Orders
                .Where(order => order.WaiterId == _loggedInWaiter.Id && order.Date.Date == SelectedDate.Date && order.State != OrderState.Ongoing)
                .Include("Table")
                .ToList()
                .ForEach(order => Orders.Add(order));
            Total = Orders.Sum(order => order.Total);
            OnPropertyChanged(nameof(SelectedDate));
        }
    }
}
