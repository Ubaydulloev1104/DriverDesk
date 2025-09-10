using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DriverDesk.Pages;

public partial class HistoryPage : ContentPage
{
    public ObservableCollection<Grouping<string, OrderVM>> GroupedOrders { get; set; } = new();

    private bool showCompletedOnly = false;
    private double currentFontScale = 1.0;

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        SetActiveTabUI();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync(string search = "")
    {
        var ordersList = await App.Database.GetOrdersAsync();
        var customers = await App.Database.GetCustomersAsync();

        var vms = ordersList.Select(o =>
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
                IsPaid = o.IsPaid,
                FontSize = 14 * currentFontScale
            };
        });

        if (showCompletedOnly)
            vms = vms.Where(x => x.IsCompleted);

        if (!string.IsNullOrWhiteSpace(search))
            vms = vms.Where(x => x.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));

        var grouped = vms
            .OrderByDescending(x => x.PickupDateTime)
            .GroupBy(x => x.PickupDateTime.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new Grouping<string, OrderVM>(
                g.Key.ToString("dd.MM.yyyy"),
                g.OrderByDescending(x => x.PickupDateTime)
            ));

        GroupedOrders.Clear();
        foreach (var grp in grouped)
            GroupedOrders.Add(grp);

        OrdersCollectionView.ItemsSource = GroupedOrders;

        TotalCountLabel.Text = $"Всего заказов: {vms.Count()}";
    }

    private async void OnAllOrdersTabClicked(object sender, EventArgs e)
    {
        showCompletedOnly = false;
        SetActiveTabUI();
        await LoadOrdersAsync(SearchBar.Text);
    }

    private async void OnCompletedOrdersTabClicked(object sender, EventArgs e)
    {
        showCompletedOnly = true;
        SetActiveTabUI();
        await LoadOrdersAsync(SearchBar.Text);
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        await LoadOrdersAsync(e.NewTextValue);
    }

    private void SetActiveTabUI()
    {
        if (AllOrdersButton == null || CompletedOrdersButton == null) return;

        if (showCompletedOnly)
        {
            CompletedOrdersButton.BackgroundColor = Colors.LightBlue;
            AllOrdersButton.BackgroundColor = Colors.Transparent;
        }
        else
        {
            AllOrdersButton.BackgroundColor = Colors.LightBlue;
            CompletedOrdersButton.BackgroundColor = Colors.Transparent;
        }
    }

    // Жест pinch
    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Running)
        {
            double scale = currentFontScale * e.Scale;
            UpdateFontSizes(scale);
        }
        else if (e.Status == GestureStatus.Completed)
        {
            currentFontScale *= e.Scale;
        }
    }

    // Кнопка "+"
    private void OnZoomInClicked(object sender, EventArgs e)
    {
        currentFontScale *= 1.1;
        UpdateFontSizes(currentFontScale);
    }

    // Кнопка "-"
    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        currentFontScale /= 1.1;
        UpdateFontSizes(currentFontScale);
    }

    // Сброс масштаба
    private void OnResetZoomClicked(object sender, EventArgs e)
    {
        currentFontScale = 1.0;
        UpdateFontSizes(currentFontScale);
    }

    private void UpdateFontSizes(double scale)
    {
        foreach (var group in GroupedOrders)
            foreach (var item in group)
                item.FontSize = 14 * scale;

        OrdersCollectionView.ItemsSource = null;
        OrdersCollectionView.ItemsSource = GroupedOrders;
    }
}

// ViewModel
public class OrderVM : INotifyPropertyChanged
{
    private double fontSize = 14;
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime PickupDateTime { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsPaid { get; set; }

    public double FontSize
    {
        get => fontSize;
        set
        {
            if (fontSize != value)
            {
                fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
    }

    public string StatusText =>
        IsCompleted ? (IsPaid ? "Выполнен и оплачен" : "Выполнен") : "Не выполнен";

    public Color StatusColor => IsCompleted ? Colors.Green : Colors.Red;

    public Color BackgroundColor
    {
        get
        {
            if (IsCompleted && IsPaid) return Colors.LightGreen;
            if (IsCompleted) return Colors.LightYellow;
            return Colors.White;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Группировка
public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; }
    public int ItemCount => Items.Count;

    public Grouping(K key, IEnumerable<T> items) : base(items)
    {
        Key = key;
    }
}
