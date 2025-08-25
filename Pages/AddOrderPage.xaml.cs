
namespace DriverDesk.Pages;

public partial class AddOrderPage : ContentPage
{
    private List<Customer> _customers = new();

    public AddOrderPage()
    {
        InitializeComponent();
        LoadCustomers();

        DatePicker.Date = DateTime.Today;
        TimePicker.Time = DateTime.Now.TimeOfDay;
    }

    private async void LoadCustomers()
    {
        _customers = await App.Database.GetCustomersAsync();
        CustomerPicker.ItemsSource = _customers;
        CustomerPicker.ItemDisplayBinding = new Binding("Name");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (CustomerPicker.SelectedItem is not Customer customer)
        {
            await DisplayAlert("Ошибка", "Выберите заказчика", "OK");
            return;
        }

        var desc = DescriptionEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Ошибка", "Введите описание заказа", "OK");
            return;
        }

        var pickup = DatePicker.Date + TimePicker.Time;

        var order = new Order
        {
            CustomerId = customer.Id,
            Description = desc,
            PickupDateTime = pickup,
            IsCompleted = false,
            IsPaid = false
        };

        await App.Database.SaveOrderAsync(order);
        await DisplayAlert("Готово", "Заказ сохранён", "OK");

        // Очистка
        CustomerPicker.SelectedItem = null;
        DescriptionEntry.Text = string.Empty;
        DatePicker.Date = DateTime.Today;
        TimePicker.Time = DateTime.Now.TimeOfDay;
    }
}
