namespace DriverDesk;

public partial class App : Application
{
    public static DatabaseService Database { get; private set; }
    public App()
    {
        InitializeComponent();
        string dbPath = Path.Combine(
      FileSystem.AppDataDirectory, "driverdesk.db3");
        Database = new DatabaseService(dbPath);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}