using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.Communication;

namespace DriverDesk.Pages;

public partial class OrdersPage : ContentPage
{
    private ObservableCollection<OrderVM> _items = new();

    public OrdersPage()
    {
        InitializeComponent();
        OrdersView.ItemsSource = _items;
        LoadData();
    }

    private async void LoadData()
    {
        var orders = await App.Database.GetOrdersAsync();
        var customers = await App.Database.GetCustomersAsync();

        var list = orders
            .OrderBy(o => o.PickupDateTime)
            .Select(o =>
            {
                var c = customers.FirstOrDefault(x => x.Id == o.CustomerId);
                return new OrderVM
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = c?.Name ?? "Неизвестно",
                    CustomerPhone = c?.Phone ?? "",
                    Description = o.Description,
                    PickupDateTime = o.PickupDateTime,
                    IsCompleted = o.IsCompleted,
                    IsPaid = o.IsPaid
                };
            })
            .ToList();

        _items.Clear();
        foreach (var item in list)
            _items.Add(item);

        UpdateStats();
    }

    private void UpdateStats()
    {
        int total = _items.Count;
        int completed = _items.Count(x => x.IsCompleted);
        int pending = total - completed;

        TotalOrdersLabel.Text = $"Всего: {total}";
        CompletedOrdersLabel.Text = $"Выполнено: {completed}";
        PendingOrdersLabel.Text = $"Не выполнено: {pending}";
    }

    private async void OnCompleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            vm.IsCompleted = true;
            await App.Database.UpdateOrderAsync(vm);
            LoadData();
        }
    }

    private async void OnPaidClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            if (!vm.IsCompleted)
            {
                await DisplayAlert("Ошибка", "Сначала отметьте заказ как выполненный", "OK");
                return;
            }

            vm.IsPaid = true;
            await App.Database.UpdateOrderAsync(vm);
            LoadData();
        }
    }

    private void OnCallClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm && !string.IsNullOrWhiteSpace(vm.CustomerPhone))
        {
            try
            {
                PhoneDialer.Open(vm.CustomerPhone);
            }
            catch
            {
                DisplayAlert("Ошибка", "Не удалось открыть телефон.", "OK");
            }
        }
    }

    private class OrderVM : Order
    {
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
    }
}
