using System;
using System.Collections.Generic;

namespace RestaurantApp.Models;

public enum OrderState
{
    Unpaid,
    Paid,
    Canceled
}

public class Order
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
    public decimal Total { get; set; } = 0;
    public OrderState State { get; set; } = OrderState.Unpaid;
    public int TableId { get; set; }
    public required Table Table { get; set; }
    public int WaiterId { get; set; }
    public required Waiter Waiter { get; set; }
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}
