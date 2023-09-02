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

    public ObservableCollection<Waiter> WaitersWithLowestMonthlyRevenue { get; set; }
    public ObservableCollection<Waiter> WaitersWithHighestMonthlyRevenue { get; set; }

    public List<Tuple<Waiter, decimal>> WaitersWithLowestRevenueOverLastSixMonths { get; set; }
    public List<Tuple<Waiter, decimal>> WaitersWithHighestRevenueOverLastSixMonths { get; set; }

    public List<Product> MostOrderedProducts { get; set; }

    public AdministratorReportsViewModel()
    {
        DailyRevenues = new(GetDailyRevenues(SelectedDate));
        MonthlyRevenues = new(GetMonthlyRevenues(SelectedYearAndMonth));

        decimal lowestRevenue = MonthlyRevenues.Min(item => item.Item2);
        WaitersWithLowestMonthlyRevenue = new(MonthlyRevenues.Where(item => item.Item2 == lowestRevenue).Select(item => item.Item1));

        decimal highestRevenue = MonthlyRevenues.Max(item => item.Item2);
        WaitersWithHighestMonthlyRevenue = new(MonthlyRevenues.Where(item => item.Item2 == highestRevenue).Select(item => item.Item1));

        InitializeRevenueListsOverLastSixMonths();

        using RestaurantContext context = new();
        MostOrderedProducts = context.Products.Where(product => product.Active).OrderByDescending(product => product.OrderCount).Take(10).ToList();
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

            decimal lowestRevenue = MonthlyRevenues.Min(item => item.Item2);
            WaitersWithLowestMonthlyRevenue.Clear();
            MonthlyRevenues.Where(item => item.Item2 == lowestRevenue)
                .Select(item => item.Item1)
                .ToList()
                .ForEach(waiter => WaitersWithLowestMonthlyRevenue.Add(waiter));

            decimal highestRevenue = MonthlyRevenues.Max(item => item.Item2);
            WaitersWithHighestMonthlyRevenue.Clear();
            MonthlyRevenues.Where(item => item.Item2 == highestRevenue)
                .Select(item => item.Item1)
                .ToList()
                .ForEach(waiter => WaitersWithHighestMonthlyRevenue.Add(waiter));

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

    private void InitializeRevenueListsOverLastSixMonths()
    {
        var revenueOverLastSixMonths = GetRevenueOverLastSixMonths();

        decimal lowestRevenue = revenueOverLastSixMonths.Min(item => item.Item2);
        WaitersWithLowestRevenueOverLastSixMonths = revenueOverLastSixMonths.Where(item => item.Item2 == lowestRevenue).ToList();

        decimal highestRevenue = revenueOverLastSixMonths.Max(item => item.Item2);
        WaitersWithHighestRevenueOverLastSixMonths = revenueOverLastSixMonths.Where(item => item.Item2 == highestRevenue).ToList();
    }

    private List<Tuple<Waiter, decimal>> GetRevenueOverLastSixMonths()
    {
        DateTime currentDate = DateTime.Now;
        List<DateTime> firstDayOfLastSixMonths = new();

        for (int i = 0; i < 6; i++)
        {
            firstDayOfLastSixMonths.Add(currentDate);
            currentDate = currentDate.AddMonths(-1);
            currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);
        }

        List<List<Tuple<Waiter, decimal>>> revenueListOverLastSixMonths = new();
        firstDayOfLastSixMonths.ForEach(date => revenueListOverLastSixMonths.Add(GetMonthlyRevenues(date)));

        List<Tuple<Waiter, decimal>> revenueOverLastSixMonths = new();
        foreach (var list in revenueListOverLastSixMonths)
        {
            foreach (var pair in list)
            {
                var existingPair = revenueOverLastSixMonths.FirstOrDefault(item => item.Item1.Id == pair.Item1.Id);
                if (existingPair == null)
                {
                    revenueOverLastSixMonths.Add(pair);
                }
                else
                {
                    existingPair = new(existingPair.Item1, existingPair.Item2 + pair.Item2);
                }
            }
        }

        return revenueOverLastSixMonths;
    }
}
