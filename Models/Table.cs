using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace RestaurantApp.Models;

public partial class Table : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private int _number;
    [ObservableProperty] private int _availableSeats;
    public bool Active { get; set; } = true;
    public int? WaiterId { get; set; }
    [ObservableProperty] private Waiter? _waiter;
    public ICollection<Order>? Orders { get; set; }
}
