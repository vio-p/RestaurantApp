using RestaurantApp.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.ViewModels;

public class WaiterViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public WaiterViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
    }
}
