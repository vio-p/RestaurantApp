using RestaurantApp.Models;
using RestaurantApp.Stores;
using RestaurantApp.ViewModels;
using RestaurantApp.Views;
using System.Linq;
using System.Windows;

namespace RestaurantApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly NavigationStore _navigationStore;

    public App()
    {
        _navigationStore = new();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        //using (RestaurantContext context = new())
        //{
        //    if (context.Users.OfType<Administrator>().SingleOrDefault(a => a.UserName == "viorica.puscas") == null)
        //    {
        //        Administrator admin = new()
        //        {
        //            UserName = "viorica.puscas",
        //            FirstName = "Viorica",
        //            LastName = "Puscas"
        //        };
        //        context.Users.Add(admin);
        //        context.SaveChanges();
        //    }
        //}

        _navigationStore.CurrentViewModel = new LogInViewModel(_navigationStore);

        MainWindow = new MainWindow()
        {
            DataContext = new MainViewModel(_navigationStore)
        };
        MainWindow.Show();

        base.OnStartup(e);
    }
}
