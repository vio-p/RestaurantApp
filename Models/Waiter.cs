using System.Collections.Generic;

namespace RestaurantApp.Models;

public class Waiter : User
{
    public bool Active { get; set; } = true;
    public ICollection<Table>? Tables { get; set; }
    public ICollection<Order>? Orders { get; set; }
}
