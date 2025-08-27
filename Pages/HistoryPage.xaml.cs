using System.Collections.ObjectModel;
using System.Linq;

namespace DriverDesk.Pages;

public partial class HistoryPage : ContentPage
{
    public ObservableCollection<Grouping<string, OrderVM>> GroupedOrders { get; set; } = new();

    private bool showCompletedOnly = false;

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadOrders();
    }

    private void LoadOrders(string search = "")
    {
        // Загружаем заказы из базы
        var orders = App.Database.GetOrdersAsync()
            .Select(o => new OrderVM
            {
                Id = o.Id,
                CustomerName = o.Customer?.Name,
                Description = o.Description,
                IsCompleted = o.IsCompleted,
                OrderDate = o.OrderDate
            });

        // Фильтр по вкладке
        if (showCompletedOnly)
            orders = orders.Where(o => o.IsCompleted);

        // Фильтр по поиску
        if (!string.IsNullOrWhiteSpace(search))
            orders = orders.Where(o =>
                !string.IsNullOrEmpty(o.CustomerName) &&
                o.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));

        // Сортировка по дате (убывание) и времени
        orders = orders.OrderByDescending(o => o.OrderDate);

        // Группировка по дате
        var grouped = orders
            .GroupBy(o => o.OrderDate.Date.ToString("dd.MM.yyyy"))
            .Select(g => new Grouping<string, OrderVM>(
                g.Key,
                g.OrderByDescending(x => x.OrderDate)));

        GroupedOrders.Clear();
        foreach (var group in grouped)
            GroupedOrders.Add(group);

        OrdersCollectionView.ItemsSource = GroupedOrders;
        TotalCountLabel.Text = $"Всего заказов: {orders.Count()}";
    }

    private void OnAllOrdersTabClicked(object sender, EventArgs e)
    {
        showCompletedOnly = false;
        LoadOrders(SearchBar.Text);
    }

    private void OnCompletedOrdersTabClicked(object sender, EventArgs e)
    {
        showCompletedOnly = true;
        LoadOrders(SearchBar.Text);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        LoadOrders(e.NewTextValue);
    }
}

// ViewModel и группа
public class OrderVM
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime OrderDate { get; set; }
}

public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; private set; }

    public Grouping(K key, IEnumerable<T> items)
    {
        Key = key;
        foreach (var item in items)
            Items.Add(item);
    }
}
