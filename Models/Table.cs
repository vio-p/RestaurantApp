using System.Collections.Generic;

namespace RestaurantApp.Models;

public class Table
{
    public int Id { get; set; }
    public required int Number { get; set; }
    public required int AvailableSeats { get; set; }
    public int OccupiedSeats { get; set; } = 0;
    public bool Active { get; set; } = true;
    public int? WaiterId { get; set; }
    public Waiter? Waiter { get; set; }
    public ICollection<Order>? Orders { get; set; }
}
