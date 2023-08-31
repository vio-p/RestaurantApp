using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace RestaurantApp.Models;

public enum OrderState
{
    Unpaid,
    Paid,
    Canceled
}

public partial class Order : ObservableObject
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
    [ObservableProperty] private decimal _total = 0;
    [ObservableProperty] private OrderState _state = OrderState.Unpaid;
    public int OccupiedSeats { get; set; }
    public int TableId { get; set; }
    public required Table Table { get; set; }
    public int WaiterId { get; set; }
    public required Waiter Waiter { get; set; }
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}
