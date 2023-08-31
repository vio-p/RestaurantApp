using Microsoft.EntityFrameworkCore;
using RestaurantApp.Commands;
using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RestaurantApp.ViewModels;

public class WaiterOrdersViewModel : ViewModelBase
{
    private readonly Waiter _loggedInWaiter;

    public List<OrderState> OrderStates { get; } = Enum.GetValues<OrderState>().ToList();

    public ObservableCollection<Table> AvailableTables { get; }
    public ObservableCollection<Order> Orders { get; }
    public ObservableCollection<OrderProduct> OrderProducts { get; }
    public ObservableCollection<Product> Products { get; }

    public ICommand StartOrderCommand { get; }
    public ICommand AddProductCommand { get; }

    public WaiterOrdersViewModel(Waiter loggedInWaiter)
    {
        _loggedInWaiter = loggedInWaiter;

        using RestaurantContext context = new();
        AvailableTables = new(context.Tables.Where(table => table.Active && table.Available));
        Orders = new(context.Orders.Include("Table")); // just the orders for the logged in waiter!!
        Products = new(context.Products.Where(product => product.Active));
        OrderProducts = new();

        StartOrderCommand = new RelayCommand(AddOrder, parameter => OrderInputIsValid());
        AddProductCommand = new RelayCommand(AddProduct, parameter => SelectedProduct != null);
    }

    private Table _selectedTable;
    public Table SelectedTable
    {
        get => _selectedTable;
        set
        {
            _selectedTable = value;
            OnPropertyChanged(nameof(SelectedTable));
        }
    }

    private string _occupiedSeats;
    public string OccupiedSeats
    {
        get => _occupiedSeats;
        set
        {
            _occupiedSeats = value;
            OnPropertyChanged(nameof(OccupiedSeats));
        }
    }

    private Visibility _productsSectionVisibility = Visibility.Hidden;
    public Visibility ProductsSectionVisibility
    {
        get => _productsSectionVisibility;
        set
        {
            _productsSectionVisibility = value;
            OnPropertyChanged(nameof(ProductsSectionVisibility));
        }
    }

    private Order _selectedOrder;
    public Order SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            _selectedOrder = value;
            ProductsSectionVisibility = _selectedOrder != null ? Visibility.Visible : Visibility.Hidden;
            OrderProducts.Clear();
            if (_selectedOrder != null)
            {
                using RestaurantContext context = new();
                context.OrderProducts
                    .Where(op => op.OrderId == _selectedOrder.Id)
                    .Include("Product")
                    .ToList()
                    .ForEach(op => OrderProducts.Add(op));
            }
            OnPropertyChanged(nameof(SelectedOrder));
        }
    }

    private Product _selectedProduct;
    public Product SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
        }
    } 

    private void AddOrder()
    {
        using RestaurantContext context = new();

        Order order = new()
        {
            Date = DateTime.Now,
            Table = context.Tables.Single(table => table.Id == SelectedTable.Id),
            Waiter = context.Users.OfType<Waiter>().Single(waiter => waiter.Id == _loggedInWaiter.Id),
            OccupiedSeats = int.Parse(OccupiedSeats)
        };
        Orders.Add(order);

        order.Table.Available = false;
        AvailableTables.Remove(SelectedTable);
        OccupiedSeats = null!;

        context.Orders.Add(order);
        context.SaveChanges();
    }

    private void AddProduct()
    {
        using RestaurantContext context = new();
        if (OrderProducts.FirstOrDefault(op => op.ProductId == SelectedProduct.Id) == null)
        {
            OrderProduct orderProduct = new()
            {
                OrderId = SelectedOrder.Id,
                ProductId = SelectedProduct.Id,
                ProductPrice = SelectedProduct.Price // what if the price of product is modified when there are orders that aren't closed and contain it, ADD RESTRICTION
            };

            context.Add(orderProduct);
            context.SaveChanges();

            orderProduct.Product = SelectedProduct;
            OrderProducts.Add(orderProduct);
        }
        else
        {
            // logic for increasing the quantity of a product
        }
    }

    private bool OrderInputIsValid()
    {
        bool occupiedSeatsIsValid = int.TryParse(OccupiedSeats, out int occupiedSeats);
        return SelectedTable != null && !string.IsNullOrEmpty(OccupiedSeats) && occupiedSeatsIsValid && occupiedSeats <= SelectedTable.AvailableSeats;
    }
}

// TODO
// add table back to list on order canceled or closed
// implement order cancel and close with necessary validations
// implement remove product
// maybe add property quantity to Product, which is decremented when product is added to order => disable product on quantity equal to 0
