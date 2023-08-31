using CommunityToolkit.Mvvm.ComponentModel;

namespace RestaurantApp.Models;

public partial class OrderProduct : ObservableObject
{
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public decimal ProductPrice { get; set; }
    [ObservableProperty] private int _quantity = 1;
    public decimal TotalPrice => ProductPrice * Quantity;
}
