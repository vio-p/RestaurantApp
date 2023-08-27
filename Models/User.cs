using CommunityToolkit.Mvvm.ComponentModel;

namespace RestaurantApp.Models;

public abstract partial class User : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] public string _username;
    [ObservableProperty] public string _firstName;
    [ObservableProperty] public string _lastName;
    public bool Active { get; set; } = true;
}
