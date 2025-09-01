using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DriverDesk.Pages;

public partial class HistoryPage : ContentPage
{
    private double _currentScale = 1.0;

    public HistoryPage()
    {
        InitializeComponent();
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // Пример данных
        var today = DateTime.Now.Date;
        OrdersCollectionView.ItemsSource = new[]
        {
            new {
                Key = today.ToString("dd.MM.yyyy"),
                Count = 2,
                Items = new[]
                {
                    new { CustomerName = "Иван Иванов", CustomerPhone="123456789", Description="Доставка документов", PickupDateTime=DateTime.Now, StatusText="Выполнен", StatusColor="Green", BackgroundColor="White" },
                    new { CustomerName = "Петр Петров", CustomerPhone="987654321", Description="Перевозка груза", PickupDateTime=DateTime.Now.AddHours(-1), StatusText="В работе", StatusColor="Orange", BackgroundColor="White" }
                }
            }
        }
        .Select(g => new Grouping<string, dynamic>(g.Key, g.Items.ToList()) { Count = g.Count })
        .ToList();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Реализуйте поиск
    }

    private void OnAllOrdersTabClicked(object sender, EventArgs e)
    {
        // Логика для всех заказов
    }

    private void OnCompletedOrdersTabClicked(object sender, EventArgs e)
    {
        // Логика для завершённых заказов
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        _currentScale += 0.1;
        ZoomContainer.Scale = _currentScale;
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        _currentScale = Math.Max(0.5, _currentScale - 0.1);
        ZoomContainer.Scale = _currentScale;
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Running)
        {
            ZoomContainer.Scale = _currentScale * e.Scale;
        }
        else if (e.Status == GestureStatus.Completed)
        {
            _currentScale = ZoomContainer.Scale;
        }
    }
}

// Вспомогательный класс для группировки
public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; private set; }
    public int Count { get; set; }
    public Grouping(K key, IEnumerable<T> items) : base(items)
    {
        Key = key;
    }
}
