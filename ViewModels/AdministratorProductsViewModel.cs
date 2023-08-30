using RestaurantApp.Commands;
using RestaurantApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
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
            }
            else
            {
                Name = null!;
                Price = null!;
            }
            OnPropertyChanged(nameof(SelectedProduct));
        }
    }

    private void AddProduct()
    {
        Product product = new()
        {
            Name = Name,
            Price = decimal.Parse(Price)
        };
        Products.Add(product);

        using RestaurantContext context = new();
        context.Products.Add(product);
        context.SaveChanges();
    }

    private void ModifyProduct()
    {
        SelectedProduct.Name = Name;
        SelectedProduct.Price = decimal.Parse(Price);

        using RestaurantContext context = new();
        Product dbProduct = context.Products.Single(product => product.Id == SelectedProduct.Id);
        dbProduct.Name = SelectedProduct.Name;
        dbProduct.Price = SelectedProduct.Price;
        context.SaveChanges();
    }

    private void DeleteProduct()
    {
        using RestaurantContext context = new();
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
