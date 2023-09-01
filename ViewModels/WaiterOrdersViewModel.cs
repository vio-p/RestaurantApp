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
    public ICommand CloseOrderCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand RemoveProductCommand { get; }

    public WaiterOrdersViewModel(Waiter loggedInWaiter)
    {
        _loggedInWaiter = loggedInWaiter;

        using RestaurantContext context = new();
        AvailableTables = new(context.Tables.Where(table => table.Active && table.Available && table.WaiterId == _loggedInWaiter.Id));
        Orders = new(context.Orders.Include("Table").Where(order => order.WaiterId == _loggedInWaiter.Id && order.State == OrderState.Unpaid));
        Products = new(context.Products.Where(product => product.Active));
        OrderProducts = new();

        StartOrderCommand = new RelayCommand(AddOrder, parameter => OrderInputIsValid());
        CloseOrderCommand = new RelayCommand(CloseOrder, parameter => SelectedOrder != null);
        CancelOrderCommand = new RelayCommand(CancelOrder, parameter => SelectedOrder != null);
        AddProductCommand = new RelayCommand(AddProduct, parameter => SelectedProduct != null);
        RemoveProductCommand = new RelayCommand(RemoveProduct, parameter => SelectedProduct != null && OrderProducts.FirstOrDefault(op => op.ProductId == SelectedProduct.Id) != null);
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

    private void CloseOrder()
    {
        using RestaurantContext context = new();
        Order dbOrder = context.Orders.Single(order => order.Id == SelectedOrder.Id);
        dbOrder.State = OrderState.Paid;

        Table dbTable = context.Tables.Single(table => table.Id == SelectedOrder.TableId);
        dbTable.Available = true;

        context.SaveChanges();

        AvailableTables.Add(dbTable);

        Orders.Remove(SelectedOrder);
        ProductsSectionVisibility = Visibility.Hidden;
    }

    private void CancelOrder()
    {
        using RestaurantContext context = new();
        Order dbOrder = context.Orders.Single(order => order.Id == SelectedOrder.Id);
        dbOrder.State = OrderState.Canceled;

        Table dbTable = context.Tables.Single(table => table.Id == SelectedOrder.TableId);
        dbTable.Available = true;

        context.SaveChanges();

        AvailableTables.Add(dbTable);

        Orders.Remove(SelectedOrder);
        ProductsSectionVisibility = Visibility.Hidden;
    }

    private void AddProduct()
    {
        using RestaurantContext context = new();
        var existingOrderProduct = OrderProducts.FirstOrDefault(op => op.ProductId == SelectedProduct.Id);
        if (existingOrderProduct == null)
        {
            OrderProduct orderProduct = new()
            {
                OrderId = SelectedOrder.Id,
                ProductId = SelectedProduct.Id,
                ProductPrice = SelectedProduct.Price // what if the price of product is modified when there are orders that aren't closed and contain it, ADD RESTRICTION
            };

            context.Add(orderProduct);

            orderProduct.Product = SelectedProduct;
            OrderProducts.Add(orderProduct);
        }
        else
        {
            OrderProduct dbOrderProduct = context.OrderProducts.Single(op => op.OrderId == SelectedOrder.Id && op.ProductId == SelectedProduct.Id);
            dbOrderProduct.Quantity++;

            existingOrderProduct.Quantity++;
        }
        Order dbOrder = context.Orders.Single(order => order.Id == SelectedOrder.Id);
        dbOrder.Total += SelectedProduct.Price;
        context.SaveChanges();

        SelectedOrder.Total += SelectedProduct.Price;
    }

    private void RemoveProduct()
    {
        using RestaurantContext context = new();
        var existingOrderProduct = OrderProducts.Single(op => op.ProductId == SelectedProduct.Id);
        if (existingOrderProduct.Quantity > 1)
        {
            existingOrderProduct.Quantity--;

            OrderProduct dbOrderProduct = context.OrderProducts.Single(op => op.OrderId == SelectedOrder.Id && op.ProductId == SelectedProduct.Id);
            dbOrderProduct.Quantity--;
        }
        else
        {
            context.Remove(existingOrderProduct);
            context.SaveChanges();

            OrderProducts.Remove(existingOrderProduct);
        }
        Order dbOrder = context.Orders.Single(order => order.Id == SelectedOrder.Id);
        dbOrder.Total -= SelectedProduct.Price;
        context.SaveChanges();

        SelectedOrder.Total -= SelectedProduct.Price;
    }

    private bool OrderInputIsValid()
    {
        bool occupiedSeatsIsValid = int.TryParse(OccupiedSeats, out int occupiedSeats);
        return SelectedTable != null && !string.IsNullOrEmpty(OccupiedSeats) && occupiedSeatsIsValid && occupiedSeats <= SelectedTable.AvailableSeats;
    }
}

// TODO
// maybe add property quantity to Product, which is decremented when product is added to order => disable product on quantity equal to 0
// check if product with same name exists and is active
// restrict user from modifying product when linked to unpaid order
