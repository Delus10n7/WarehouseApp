namespace WarehouseApp.Maui;

/// <summary>
/// Главный класс приложения, управляющий его жизненным циклом и созданием главного окна.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="App"/>.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Создает главное окно приложения и устанавливает стартовую навигацию (AppShell).
    /// </summary>
    /// <param name="activationState">Состояние активации приложения.</param>
    /// <returns>Объект главного окна <see cref="Window"/>.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
