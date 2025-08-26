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
                    CustomerName = c?.Name ?? "����������",
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

        TotalOrdersLabel.Text = $"�����: {total}";
        CompletedOrdersLabel.Text = $"���������: {completed}";
        PendingOrdersLabel.Text = $"�� ���������: {pending}";
    }

    private async void OnCompleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            vm.IsCompleted = true;

            // ��������� ������ ������ Order ��� SQLite
            var order = new Order
            {
                Id = vm.Id,
                CustomerId = vm.CustomerId,
                Description = vm.Description,
                PickupDateTime = vm.PickupDateTime,
                IsCompleted = vm.IsCompleted,
                IsPaid = vm.IsPaid
            };

            await App.Database.UpdateOrderAsync(order);
            LoadData();
        }
    }

    private async void OnPaidClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            if (!vm.IsCompleted)
            {
                await DisplayAlert("������", "������� �������� ����� ��� �����������", "OK");
                return;
            }

            vm.IsPaid = true;

            var order = new Order
            {
                Id = vm.Id,
                CustomerId = vm.CustomerId,
                Description = vm.Description,
                PickupDateTime = vm.PickupDateTime,
                IsCompleted = vm.IsCompleted,
                IsPaid = vm.IsPaid
            };

            await App.Database.UpdateOrderAsync(order);
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
                DisplayAlert("������", "�� ������� ������� �������.", "OK");
            }
        }
    }

    // ��������������� ����� ��� �����������
    private class OrderVM
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime PickupDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPaid { get; set; }
    }
}
