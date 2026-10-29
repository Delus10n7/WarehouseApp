using CommunityToolkit.Mvvm.Input;

namespace WarehouseApp.Maui.Controls;

/// <summary>
/// Кастомный нижний таб-бар приложения с 4 вкладками.
/// Навигация — через абсолютные маршруты Shell (<c>//route</c>).
/// </summary>
public partial class BottomTabBar : ContentView
{
    /// <summary>Bindable-свойство для идентификатора активной вкладки.</summary>
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomTabBar), string.Empty);

    /// <summary>
    /// Идентификатор активной вкладки: "reports", "documents", "products" или "counterparties".
    /// Используется в XAML-триггерах для подсветки активной иконки.
    /// </summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    /// <summary>Команда перехода на вкладку по её маршруту.</summary>
    public IAsyncRelayCommand<string> OnTabTappedCommand { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BottomTabBar"/>.
    /// </summary>
    public BottomTabBar()
    {
        OnTabTappedCommand = new AsyncRelayCommand<string>(OnTabTappedAsync);
        InitializeComponent();
    }

    /// <summary>
    /// Переходит на вкладку Shell по абсолютному маршруту.
    /// </summary>
    /// <param name="route">Маршрут вкладки (например, "products").</param>
    private async Task OnTabTappedAsync(string? route)
    {
        if (string.IsNullOrEmpty(route) || route == ActiveTab) return;
        await Shell.Current.GoToAsync($"//{route}");
    }
}
