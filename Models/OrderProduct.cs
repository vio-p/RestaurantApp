namespace RestaurantApp.Models;

public class OrderProduct
{
    public int OrderId { get; set; }
    public required Order Order { get; set; }
    public int ProductId { get; set; }
    public required Product Product { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}
