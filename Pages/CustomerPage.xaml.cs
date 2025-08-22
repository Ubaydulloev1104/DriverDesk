namespace DriverDesk.Pages;

public partial class CustomerPage : ContentPage
{
    public CustomerPage()
    {
        InitializeComponent();
        LoadCustomers();
    }

    private async void LoadCustomers()
    {
        CustomerList.ItemsSource = await App.Database.GetCustomersAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            var customer = new Customer
            {
                Name = NameEntry.Text,
                Phone = PhoneEntry.Text
            };
            await App.Database.SaveCustomerAsync(customer);

            NameEntry.Text = string.Empty;
            PhoneEntry.Text = string.Empty;

            LoadCustomers();
        }
    }

    private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Customer customer)
        {
            bool confirm = await DisplayAlert("Удалить?", $"Удалить {customer.Name}?", "Да", "Нет");
            if (confirm)
            {
                await App.Database.DeleteCustomerAsync(customer); // нужно будет сделать метод для Customers
                LoadCustomers();
            }
        }
    }
}

