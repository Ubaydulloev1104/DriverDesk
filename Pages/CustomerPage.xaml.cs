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
        var name = NameEntry.Text?.Trim();
        var phone = PhoneEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Ошибка", "Введите имя", "OK");
            return;
        }

        var customer = new Customer { Name = name, Phone = phone };
        await App.Database.SaveCustomerAsync(customer);

        NameEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        LoadCustomers();
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Customer customer)
        {
            bool del = await DisplayAlert("Удалить?", $"Удалить {customer.Name}?", "Да", "Нет");
            if (del)
            {
                await App.Database.DeleteCustomerAsync(customer);
                LoadCustomers();
            }
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}

