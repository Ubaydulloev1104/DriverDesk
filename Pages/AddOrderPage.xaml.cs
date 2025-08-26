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
        Customer? customer = CustomerPicker.SelectedItem as Customer;

        // Если заказчик не выбран, но введены данные нового
        if (customer == null &&
            !string.IsNullOrWhiteSpace(NewCustomerNameEntry.Text) &&
            !string.IsNullOrWhiteSpace(NewCustomerPhoneEntry.Text))
        {
            customer = new Customer
            {
                Name = NewCustomerNameEntry.Text.Trim(),
                Phone = NewCustomerPhoneEntry.Text.Trim()
            };

            await App.Database.SaveCustomerAsync(customer);

            // Перезагружаем список и выбираем нового
            await LoadCustomersAndSelect(customer);
        }

        if (customer == null)
        {
            await DisplayAlert("Ошибка", "Выберите заказчика или введите нового.", "OK");
            return;
        }

        var desc = DescriptionEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("Ошибка", "Введите описание заказа.", "OK");
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

        await DisplayAlert("Готово", "Заказ сохранён.", "OK");
        ClearForm();
    }

    private async Task LoadCustomersAndSelect(Customer newCustomer)
    {
        _customers = await App.Database.GetCustomersAsync();
        CustomerPicker.ItemsSource = _customers;
        CustomerPicker.SelectedItem = _customers.FirstOrDefault(c => c.Id == newCustomer.Id);
    }

    private void ClearForm()
    {
        CustomerPicker.SelectedItem = null;
        NewCustomerNameEntry.Text = string.Empty;
        NewCustomerPhoneEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        DatePicker.Date = DateTime.Today;
        TimePicker.Time = DateTime.Now.TimeOfDay;
    }
}
