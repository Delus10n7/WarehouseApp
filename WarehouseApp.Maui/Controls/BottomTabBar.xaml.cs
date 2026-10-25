namespace WarehouseApp.Maui.Controls;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// Кастомный элемент управления - нижняя панель навигации (таб-бар).
/// </summary>
public partial class BottomTabBar : ContentView
{
    /// <summary>
    /// Свойство зависимости для хранения текущей активной вкладки.
    /// </summary>
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomTabBar), string.Empty);

    /// <summary>
    /// Строковый идентификатор текущей активной вкладки (например, "reports", "products").
    /// </summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    /// <summary>
    /// Команда, выполняемая при нажатии на одну из вкладок.
    /// </summary>
    public IAsyncRelayCommand<string> OnTabTappedCommand { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BottomTabBar"/>.
    /// </summary>
    public BottomTabBar()
    {
        OnTabTappedCommand = new AsyncRelayCommand<string>(OnTabTappedAsync);
        InitializeComponent();
    }

    /// <summary>
    /// Выполняет навигацию на выбранную вкладку.
    /// </summary>
    /// <param name="route">Маршрут вкладки.</param>
    private async Task OnTabTappedAsync(string? route)
    {
        // Игнорируем нажатие, если маршрут пуст или уже активен
        if (string.IsNullOrEmpty(route) || route == ActiveTab) return;

        // Используем абсолютный маршрут для переключения между вкладками Shell
        await Shell.Current.GoToAsync($"//{route}");
    }
}
