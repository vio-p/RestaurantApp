using System.Collections.Generic;

namespace RestaurantApp.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public bool Active { get; set; } = true;
    public int OrderCount { get; set; } = 0;
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}
