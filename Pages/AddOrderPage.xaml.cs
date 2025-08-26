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

    // ������� "������� ���������"
    private async void OnSelectCustomerClicked(object sender, EventArgs e)
    {
        _customers = await App.Database.GetCustomersAsync();

        // ��������� ����� �������� (��� DisplayActionSheet)
        var names = _customers.Select(c => c.Name).ToArray();
        string action = await DisplayActionSheet("�������� ���������", "������", null, names);

        if (!string.IsNullOrEmpty(action) && action != "������")
        {
            var selectedCustomer = _customers.FirstOrDefault(c => c.Name == action);
            if (selectedCustomer != null)
            {
                NewCustomerNameEntry.Text = selectedCustomer.Name;
                NewCustomerPhoneEntry.Text = selectedCustomer.Phone;
            }
        }
    }

    // ������� "���������"
    private async void OnCallClicked(object sender, EventArgs e)
    {
        var phone = NewCustomerPhoneEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(phone))
        {
            try
            {
                // ���������� ���������� API ��� �������
                await Launcher.OpenAsync(new Uri($"tel:{phone}"));
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"�� ������� ������� ��������: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("������", "������� ����� ��������.", "OK");
        }
    }

    // ���������� ������ (��������� ����� ��� ���������)
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        Customer? customer = null;

        // ���������, ���� �� ������ � �����
        if (!string.IsNullOrWhiteSpace(NewCustomerNameEntry.Text) &&
            !string.IsNullOrWhiteSpace(NewCustomerPhoneEntry.Text))
        {
            // ���� ������������� ���������
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
            await DisplayAlert("������", "�������� ��������� ��� ������� ������.", "OK");
            return;
        }

        var desc = DescriptionEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(desc))
        {
            await DisplayAlert("������", "������� �������� ������.", "OK");
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
        await DisplayAlert("������", "����� �������.", "OK");
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
