namespace WarehouseApp.Maui;

/// <summary>
/// Shell приложения: регистрирует маршруты вкладок и создаёт стартовую навигацию.
/// </summary>
/// <remarks>
/// Стандартный TabBar скрыт (<c>Shell.TabBarIsVisible="False"</c>), потому что
/// используется кастомный <see cref="Controls.BottomTabBar"/>. При этом
/// <c>ShellContent</c> в <c>TabBar</c> остаются нужны — они регистрируют маршруты
/// <c>//reports</c>, <c>//documents</c>, <c>//products</c>, <c>//counterparties</c>,
/// на которые ссылается <c>BottomTabBar.OnTabTappedAsync</c>.
/// </remarks>
public partial class AppShell : Shell
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AppShell"/>.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
    }
}
