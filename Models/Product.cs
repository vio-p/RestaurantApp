using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace RestaurantApp.Models;

public partial class Product : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private string _name;
    [ObservableProperty] private decimal _price;
    public bool Active { get; set; } = true;
    public int OrderCount { get; set; } = 0;
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}
