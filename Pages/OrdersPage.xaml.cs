using System.Collections.ObjectModel;

namespace DriverDesk.Pages;

public partial class OrdersPage : ContentPage
{
    private ObservableCollection<OrderGroup> _groups = new();

    public OrdersPage()
    {
        InitializeComponent();
        OrdersView.ItemsSource = _groups;
        LoadData();
    }

    private async void LoadData()
    {
        var orders = await App.Database.GetOrdersAsync();
        var customers = await App.Database.GetCustomersAsync();
        var now = DateTime.Now;

        // Фильтруем
        var filtered = orders
            .Where(o =>
            {
                if (o.IsCompleted && o.IsPaid) return false;
                if (o.IsCompleted && !o.IsPaid && (now - o.PickupDateTime).TotalHours > 12) return false;
                return true;
            })
            .Select(o =>
            {
                var c = customers.FirstOrDefault(x => x.Id == o.CustomerId);
                var vm = new OrderVM
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
                vm.UpdateProperties();
                return vm;
            })
            .OrderBy(o => o.PickupDateTime)
            .ToList();

        // Группируем по дате
        var grouped = filtered
            .GroupBy(o => o.PickupDateTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new OrderGroup(g.Key, new ObservableCollection<OrderVM>(g)))
            .ToList();

        _groups.Clear();
        foreach (var group in grouped) _groups.Add(group);

        UpdateStats(filtered);
    }

    private void UpdateStats(List<OrderVM> list)
    {
        int total = list.Count;
        int completed = list.Count(x => x.IsCompleted);
        int pending = total - completed;
      
    }

    private async void OnCompleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            vm.IsCompleted = true;
            vm.UpdateProperties();
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
            vm.UpdateProperties();
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

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm)
        {
            bool confirm = await DisplayAlert("Удаление",
                $"Удалить заказ '{vm.Description}'?",
                "Да", "Нет");

            if (confirm)
            {
                await App.Database.DeleteOrderAsync(vm.Id);
                LoadData();
            }
        }
    }



   
    private async void OnCallClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is OrderVM vm && !string.IsNullOrWhiteSpace(vm.CustomerPhone))
        {
           await Launcher.OpenAsync(new Uri($"tel:{vm.CustomerPhone}"));   
        }
    }

    public class OrderGroup : ObservableCollection<OrderVM>
    {
        public DateTime Key { get; }
        public OrderGroup(DateTime key, ObservableCollection<OrderVM> items) : base(items)
        {
            Key = key;
        }
    }

    public class OrderVM
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime PickupDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPaid { get; set; }
        public bool ShowCompleteButton { get; set; }
        public bool ShowPaidButton { get; set; }
        public bool ShowDeleteButton { get; set; }
        public Color BackgroundColor { get; set; } = Colors.White;

        public void UpdateProperties()
        {
            ShowCompleteButton = !IsCompleted;
            ShowPaidButton = IsCompleted && !IsPaid;
            ShowDeleteButton = !IsCompleted && !IsPaid;

            if (IsCompleted && IsPaid)
                BackgroundColor = Colors.LightGreen;
            else if (IsCompleted)
                BackgroundColor = Colors.LightYellow;
            else
                BackgroundColor = Colors.White;
        }
    }
}
