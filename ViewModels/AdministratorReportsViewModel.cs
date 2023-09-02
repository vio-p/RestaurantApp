using Microsoft.EntityFrameworkCore;
using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;

namespace RestaurantApp.ViewModels;

class AdministratorReportsViewModel : ViewModelBase
{
    public ObservableCollection<Tuple<Waiter, decimal>> DailyRevenues { get; }
    public ObservableCollection<Tuple<Waiter, decimal>> MonthlyRevenues { get; }
    public AdministratorReportsViewModel()
    {
        DailyRevenues = new(GetDailyRevenues(SelectedDate));
        MonthlyRevenues = new(GetMonthlyRevenues(SelectedYearAndMonth));
    }

    private DateTime _selectedDate = DateTime.Now;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;

            DailyRevenues.Clear();
            var result = GetDailyRevenues(_selectedDate);
            result.ToList().ForEach(item => DailyRevenues.Add(item));

            OnPropertyChanged(nameof(SelectedDate));
        }
    }

    private DateTime _selectedYearAndMonth = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    public DateTime SelectedYearAndMonth
    {
        get => _selectedYearAndMonth;
        set
        {
            _selectedYearAndMonth = value;
            _selectedYearAndMonth = new(_selectedYearAndMonth.Year, _selectedYearAndMonth.Month, 1);

            MonthlyRevenues.Clear();
            var result = GetMonthlyRevenues(_selectedYearAndMonth);
            result.ToList().ForEach(item => MonthlyRevenues.Add(item));

            OnPropertyChanged(nameof(SelectedYearAndMonth));
        }
    }

    private List<Tuple<Waiter, decimal>> GetDailyRevenues(DateTime date)
    {
        using RestaurantContext context = new();
        var result = from waiter in context.Users.OfType<Waiter>().Where(user => user.Active)
                     join order in context.Orders.Where(order => order.State == OrderState.Finished && order.Date.Date == date.Date)
                     on waiter.Id equals order.WaiterId into ordersGroup
                     from order in ordersGroup.DefaultIfEmpty() // perform a left join here
                     group order by waiter into waiterOrders
                     select new Tuple<Waiter, decimal>(
                         waiterOrders.Key,
                         waiterOrders.Sum(o => o != null ? o.Total : 0)
                     );

        List<Tuple<Waiter, decimal>> dailyRevenues = new();

        result.ToList().ForEach(item => dailyRevenues.Add(item));
        return dailyRevenues;
    }

    private List<Tuple<Waiter, decimal>> GetMonthlyRevenues(DateTime date)
    {
        using RestaurantContext context = new();
        var result = from waiter in context.Users.OfType<Waiter>().Where(user => user.Active)
                     join order in context.Orders.Where(order => order.State == OrderState.Finished && order.Date.Year == date.Year && order.Date.Month == date.Month)
                     on waiter.Id equals order.WaiterId into ordersGroup
                     from order in ordersGroup.DefaultIfEmpty() // perform a left join here
                     group order by waiter into waiterOrders
                     select new Tuple<Waiter, decimal>(
                         waiterOrders.Key,
                         waiterOrders.Sum(o => o != null ? o.Total : 0)
                     );

        List<Tuple<Waiter, decimal>> monthlyRevenues = new();

        result.ToList().ForEach(item => monthlyRevenues.Add(item));
        return monthlyRevenues;
    }
}
