using System.Collections.ObjectModel;


namespace DriverDesk.Pages;

public partial class DebtorsPage : ContentPage
{
    private ObservableCollection<OrderVM> _items = new();

    public DebtorsPage()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        var orders = await App.Database.GetOrdersAsync();
        var customers = await App.Database.GetCustomersAsync();

        var list = orders
            .Where(o => o.IsCompleted && !o.IsPaid)
            .OrderBy(o => o.PickupDateTime)
            .Select(o =>
            {
                var c = customers.FirstOrDefault(x => x.Id == o.CustomerId);
                return new OrderVM
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = c?.Name ?? "Неизвестно",
                    Description = o.Description,
                    PickupDateTime = o.PickupDateTime,
                    IsCompleted = o.IsCompleted,
                    IsPaid = o.IsPaid
                };
            })
            .ToList();

        _items = new ObservableCollection<OrderVM>(list);
        DebtorsView.ItemsSource = _items;
    }

    private async void OnPaidClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            vm.IsPaid = true;
            await App.Database.UpdateOrderAsync(new Order
            {
                Id = vm.Id,
                CustomerId = vm.CustomerId,
                Description = vm.Description,
                PickupDateTime = vm.PickupDateTime,
                IsCompleted = vm.IsCompleted,
                IsPaid = vm.IsPaid
            });
            LoadData();
        }
    }

    private class OrderVM : Order
    {
        public string CustomerName { get; set; } = "";
    }
}
