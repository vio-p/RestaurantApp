using Microsoft.EntityFrameworkCore;
using RestaurantApp.Commands;
using RestaurantApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace RestaurantApp.ViewModels;

public class AdministratorProductsViewModel : ViewModelBase
{
    public ObservableCollection<Product> Products { get; }

    public ICommand AddProductCommand { get; }
    public ICommand ModifyProductCommand { get; }
    public ICommand DeleteProductCommand { get; }

    public AdministratorProductsViewModel()
    {
        using RestaurantContext context = new();
        Products = new(context.Products.Where(product => product.Active).ToList());

        AddProductCommand = new RelayCommand(AddProduct, parameter => InputIsValid());
        ModifyProductCommand = new RelayCommand(ModifyProduct, parameter => InputIsValid() && SelectedProduct != null);
        DeleteProductCommand = new RelayCommand(DeleteProduct, parameter => SelectedProduct != null);
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private string _price;
    public string Price
    {
        get => _price;
        set
        {
            _price = value;
            OnPropertyChanged(nameof(Price));
        }
    }

    private bool _available = true;
    public bool Available
    {
        get => _available;
        set
        {
            _available = value;
            OnPropertyChanged(nameof(Available));
        }
    }

    private Product _selectedProduct;
    public Product SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            if (_selectedProduct != null)
            {
                Name = _selectedProduct.Name;
                Price = _selectedProduct.Price.ToString();
                Available = _selectedProduct.Available;
            }
            else
            {
                Name = null!;
                Price = null!;
                Available = true;
            }
            OnPropertyChanged(nameof(SelectedProduct));
        }
    }

    private void AddProduct()
    {
        using RestaurantContext context = new();
        if (context.Products.SingleOrDefault(product => product.Name == Name && product.Active) != null)
        {
            _ = MessageBox.Show("There is already a product with this name!", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Product product = new()
        {
            Name = Name,
            Price = decimal.Parse(Price),
            Available = Available
        };
        Products.Add(product);

        context.Products.Add(product);
        context.SaveChanges();
    }

    private void ModifyProduct()
    {
        using RestaurantContext context = new();

        List<Order> unpaidOrdersLinkedToProduct = context.Orders
            .Join(context.OrderProducts,
                  order => order.Id,
                  orderProduct => orderProduct.OrderId,
                  (order, orderProduct) => new { order, orderProduct })
            .Where(joinResult => joinResult.order.State == OrderState.Unpaid && joinResult.orderProduct.ProductId == SelectedProduct.Id)
            .Select(joinResult => joinResult.order).ToList();
        if (unpaidOrdersLinkedToProduct.Count > 0)
        {
            _ = MessageBox.Show("This product can't be modified because it is linked to an ongoing order!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedProduct.Name = Name;
        SelectedProduct.Price = decimal.Parse(Price);
        SelectedProduct.Available = Available;

        Product dbProduct = context.Products.Single(product => product.Id == SelectedProduct.Id);
        dbProduct.Name = SelectedProduct.Name;
        dbProduct.Price = SelectedProduct.Price;
        dbProduct.Available = SelectedProduct.Available;
        context.SaveChanges();
    }

    private void DeleteProduct()
    {
        using RestaurantContext context = new();
        List<Order> unpaidOrdersLinkedToProduct = context.Orders
            .Join(context.OrderProducts,
                  order => order.Id,
                  orderProduct => orderProduct.OrderId,
                  (order, orderProduct) => new { order, orderProduct })
            .Where(joinResult => joinResult.order.State == OrderState.Unpaid && joinResult.orderProduct.ProductId == SelectedProduct.Id)
            .Select(joinResult => joinResult.order).ToList();
        if (unpaidOrdersLinkedToProduct.Count > 0)
        {
            _ = MessageBox.Show("This product can't be modified because it is linked to an ongoing order!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Product dbProduct = context.Products.Single(product => product.Id == SelectedProduct.Id);
        dbProduct.Active = false;
        context.SaveChanges();

        Products.Remove(SelectedProduct);
    }

    private bool InputIsValid()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Price))
        {
            return false;
        }
        bool priceIsValid = decimal.TryParse(Price, out _);
        return priceIsValid;
    }
}
