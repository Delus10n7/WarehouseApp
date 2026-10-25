namespace WarehouseApp.Maui;

/// <summary>
/// Стартовая (демонстрационная) страница приложения.
/// </summary>
public partial class MainPage : ContentPage
{
    int count = 0;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainPage"/>.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обработчик нажатия на кнопку. Увеличивает счетчик и обновляет текст на кнопке.
    /// </summary>
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
