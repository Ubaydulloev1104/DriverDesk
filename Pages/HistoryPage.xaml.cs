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
        // ������ ������
        var today = DateTime.Now.Date;
        OrdersCollectionView.ItemsSource = new[]
        {
            new {
                Key = today.ToString("dd.MM.yyyy"),
                Count = 2,
                Items = new[]
                {
                    new { CustomerName = "���� ������", CustomerPhone="123456789", Description="�������� ����������", PickupDateTime=DateTime.Now, StatusText="��������", StatusColor="Green", BackgroundColor="White" },
                    new { CustomerName = "���� ������", CustomerPhone="987654321", Description="��������� �����", PickupDateTime=DateTime.Now.AddHours(-1), StatusText="� ������", StatusColor="Orange", BackgroundColor="White" }
                }
            }
        }
        .Select(g => new Grouping<string, dynamic>(g.Key, g.Items.ToList()) { Count = g.Count })
        .ToList();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // ���������� �����
    }

    private void OnAllOrdersTabClicked(object sender, EventArgs e)
    {
        // ������ ��� ���� �������
    }

    private void OnCompletedOrdersTabClicked(object sender, EventArgs e)
    {
        // ������ ��� ����������� �������
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

// ��������������� ����� ��� �����������
public class Grouping<K, T> : ObservableCollection<T>
{
    public K Key { get; private set; }
    public int Count { get; set; }
    public Grouping(K key, IEnumerable<T> items) : base(items)
    {
        Key = key;
    }
}
