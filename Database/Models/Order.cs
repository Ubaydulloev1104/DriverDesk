using SQLite;
public class Order
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CustomerId { get; set; }

    public string Description { get; set; } // например "1 штук", "1 рез полный"
    public DateTime PickupDateTime { get; set; } // дата + время

    public bool IsPaid { get; set; } = false; // отметка оплаты
}