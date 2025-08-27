using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace DriverDesk.Pages;

public partial class HistoryPage : ContentPage
{
    // Коллекция групп для привязки к CollectionView
    public ObservableCollection<Grouping<string, OrderVM>> GroupedOrders { get; set; } = new();

    private bool showCompletedOnly = false;

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Установим начальную подсветку вкладок
        SetActiveTabUI();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    // core: загрузка и подготовка данных
    private async Task LoadOrdersAsync(string search = "")
    {
        // Получаем данные из базы
        var ordersList = await App.Database.GetOrdersAsync();         // Task<List<Order>>
        var customers = await App.Database.GetCustomersAsync();      // Task<List<Customer>>
        var now = DateTime.Now;

        // Преобразуем в OrderVM, сопоставляя заказчика через CustomerId
        var vms = ordersList
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
            .AsEnumerable();

        // Применяем вкладку (фильтрация)
        if (showCompletedOnly)
            vms = vms.Where(x => x.IsCompleted);

        // Фильтр поиска по имени клиента
        if (!string.IsNullOrWhiteSpace(search))
            vms = vms.Where(x => !string.IsNullOrEmpty(x.CustomerName)
                                  && x.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));

        // Сортировка: сначала группы по дате (убывание: новые сверху),
        // внутри группы — по времени (убывание)
        var vmsList = vms.ToList();

        var grouped = vmsList
            .OrderByDescending(x => x.PickupDateTime) // сначала по дате/времени
            .GroupBy(x => x.PickupDateTime.Date)      // группируем по дате (DateTime.Date)
            .OrderByDescending(g => g.Key)            // группы: новые даты сверху
            .Select(g =>
                new Grouping<string, OrderVM>(
                    g.Key.ToString("dd.MM.yyyy"),      // ключ-группы — строка даты
                    g.OrderByDescending(x => x.PickupDateTime) // элементы в группе: по времени убыв.
                )
            )
            .ToList();

        // Обновляем коллекцию привязанную к UI
        GroupedOrders.Clear();
        foreach (var grp in grouped)
            GroupedOrders.Add(grp);

        OrdersCollectionView.ItemsSource = GroupedOrders;

        // Общее число (после применения фильтров)
        TotalCountLabel.Text = $"Всего заказов: {vmsList.Count}";
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

    // Визуально выделяет активную вкладку
    private void SetActiveTabUI()
    {
        if (AllOrdersButton is null || CompletedOrdersButton is null) return;

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
}

// ViewModel (для отображения)
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

    // UI-дружественные свойства
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
}

// Группировка для CollectionView
public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; }
    public int Count => this.Items.Count;

    public Grouping(K key, IEnumerable<T> items) : base(items)
    {
        Key = key;
    }
}
