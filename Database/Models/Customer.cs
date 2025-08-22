using SQLite;
public class Customer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; }
}