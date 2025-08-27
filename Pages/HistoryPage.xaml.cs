using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace DriverDesk.Pages;

public partial class HistoryPage : ContentPage
{
    // ��������� ����� ��� �������� � CollectionView
    public ObservableCollection<Grouping<string, OrderVM>> GroupedOrders { get; set; } = new();

    private bool showCompletedOnly = false;

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;

        // ��������� ��������� ��������� �������
        SetActiveTabUI();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    // core: �������� � ���������� ������
    private async Task LoadOrdersAsync(string search = "")
    {
        // �������� ������ �� ����
        var ordersList = await App.Database.GetOrdersAsync();         // Task<List<Order>>
        var customers = await App.Database.GetCustomersAsync();      // Task<List<Customer>>
        var now = DateTime.Now;

        // ����������� � OrderVM, ����������� ��������� ����� CustomerId
        var vms = ordersList
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
            .AsEnumerable();

        // ��������� ������� (����������)
        if (showCompletedOnly)
            vms = vms.Where(x => x.IsCompleted);

        // ������ ������ �� ����� �������
        if (!string.IsNullOrWhiteSpace(search))
            vms = vms.Where(x => !string.IsNullOrEmpty(x.CustomerName)
                                  && x.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));

        // ����������: ������� ������ �� ���� (��������: ����� ������),
        // ������ ������ � �� ������� (��������)
        var vmsList = vms.ToList();

        var grouped = vmsList
            .OrderByDescending(x => x.PickupDateTime) // ������� �� ����/�������
            .GroupBy(x => x.PickupDateTime.Date)      // ���������� �� ���� (DateTime.Date)
            .OrderByDescending(g => g.Key)            // ������: ����� ���� ������
            .Select(g =>
                new Grouping<string, OrderVM>(
                    g.Key.ToString("dd.MM.yyyy"),      // ����-������ � ������ ����
                    g.OrderByDescending(x => x.PickupDateTime) // �������� � ������: �� ������� ����.
                )
            )
            .ToList();

        // ��������� ��������� ����������� � UI
        GroupedOrders.Clear();
        foreach (var grp in grouped)
            GroupedOrders.Add(grp);

        OrdersCollectionView.ItemsSource = GroupedOrders;

        // ����� ����� (����� ���������� ��������)
        TotalCountLabel.Text = $"����� �������: {vmsList.Count}";
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

    // ��������� �������� �������� �������
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

// ViewModel (��� �����������)
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

    // UI-������������� ��������
    public string StatusText =>
        IsCompleted ? (IsPaid ? "�������� � �������" : "��������") : "�� ��������";

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

// ����������� ��� CollectionView
public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; }
    public int Count => this.Items.Count;

    public Grouping(K key, IEnumerable<T> items) : base(items)
    {
        Key = key;
    }
}
