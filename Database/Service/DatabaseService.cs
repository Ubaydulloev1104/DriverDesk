using SQLite;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Customer>().Wait();
        _database.CreateTableAsync<Order>().Wait();
    }

    // Customers
    public Task<List<Customer>> GetCustomersAsync() =>
        _database.Table<Customer>().ToListAsync();

    public Task<int> SaveCustomerAsync(Customer customer) =>
        _database.InsertAsync(customer);
    public Task<int> UpdateCustomerAsync(Customer customer) => _database.UpdateAsync(customer);
    public Task<int> DeleteCustomerAsync(Customer customer) => _database.DeleteAsync(customer);

    // Orders
    public Task<List<Order>> GetOrdersAsync() =>
        _database.Table<Order>().ToListAsync();

    public Task<int> SaveOrderAsync(Order order) =>
        _database.InsertAsync(order);

    public Task<int> DeleteOrderAsync(Order order) =>
        _database.DeleteAsync(order);
    public Task<int> UpdateOrderAsync(Order order) => _database.UpdateAsync(order);
    public Task<List<Order>> GetOrdersByDateAsync(DateTime date) =>
        _database.Table<Order>()
        .Where(o => o.PickupDateTime.Date == date.Date)
        .OrderBy(o => o.PickupDateTime)
        .ToListAsync();
}