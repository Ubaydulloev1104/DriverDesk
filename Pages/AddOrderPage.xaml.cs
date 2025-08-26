namespace DriverDesk.Pages;

public partial class AddOrderPage : ContentPage
{
    private List<Customer> _customers = new();

    public AddOrderPage()
    {
        InitializeComponent();
        DatePicker.Date = DateTime.Today;
        TimePicker.Time = DateTime.Now.TimeOfDay;
    }

    // Нажатие "Выбрать заказчика"
    private async void OnSelectCustomerClicked(object sender, EventArgs e)
    {
        _customers = await App.Database.GetCustomersAsync();

        // Открываем новую страницу (или DisplayActionSheet)
        var names = _customers.Select(c => c.Name).ToArray();
        string action = await DisplayActionSheet("Выберите заказчика", "Отмена", null, names);

        if (!string.IsNullOrEmpty(action) && action != "Отмена")
        {
            var selectedCustomer = _customers.FirstOrDefault(c => c.Name == action);
            if (selectedCustomer != null)
            {
                NewCustomerNameEntry.Text = selectedCustomer.Name;
                NewCustomerPhoneEntry.Text = selectedCustomer.Phone;
            }
        }
    }

    // Нажатие "Позвонить"
    private async void OnCallClicked(object sender, EventArgs e)
    {
        var phone = NewCustomerPhoneEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(phone))
        {
            try
            {
                // Используем встроенный API для звонков
                await Launcher.OpenAsync(new Uri($"tel:{phone}"));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть звонилку: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Введите номер телефона.", "OK");
        }
    }

    // Сохранение заказа (оставляем почти без изменений)
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        Customer? customer = null;

        // Проверяем, есть ли данные в полях
        if (!string.IsNullOrWhiteSpace(NewCustomerNameEntry.Text) &&
            !string.IsNullOrWhiteSpace(NewCustomerPhoneEntry.Text))
        {
            // Ищем существующего заказчика
            _customers = await App.Database.GetCustomersAsync();
            customer = _customers.FirstOrDefault(c =>
                c.Name == NewCustomerNameEntry.Text.Trim() &&
                c.Phone == NewCustomerPhoneEntry.Text.Trim());

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = NewCustomerNameEntry.Text.Trim(),
                    Phone = NewCustomerPhoneEntry.Text.Trim()
                };
                await App.Database.SaveCustomerAsync(customer);
            }
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

    private void ClearForm()
    {
        NewCustomerNameEntry.Text = string.Empty;
        NewCustomerPhoneEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        DatePicker.Date = DateTime.Today;
        TimePicker.Time = DateTime.Now.TimeOfDay;
    }
}
