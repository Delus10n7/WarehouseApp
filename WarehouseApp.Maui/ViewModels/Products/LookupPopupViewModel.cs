using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel для универсального всплывающего окна добавления справочника
/// (категории или единицы измерения).
/// </summary>
public partial class LookupPopupViewModel : ObservableObject
{
    /// <summary>Заголовок окна (например, «Создание категории товара»).</summary>
    [ObservableProperty] private string title = string.Empty;

    /// <summary>Placeholder для поля ввода (например, «Введите название категории...»).</summary>
    [ObservableProperty] private string placeholder = string.Empty;

    /// <summary>Текст на кнопке сохранения (например, «Добавить категорию»).</summary>
    [ObservableProperty] private string buttonText = string.Empty;

    /// <summary>Введённое пользователем значение (название категории / единицы).</summary>
    [ObservableProperty] private string name = string.Empty;

    /// <summary>
    /// Callback, вызываемый при сохранении. Принимает введённую строку (без пробелов по краям).
    /// </summary>
    public Func<string, Task>? OnSave { get; set; }

    /// <summary>
    /// Callback для закрытия попапа. Устанавливается из code-behind
    /// (<c>LookupPopup.xaml.cs</c>) — обычно <c>async () =&gt; await CloseAsync()</c>.
    /// </summary>
    public Func<Task>? CloseAction { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LookupPopupViewModel"/>.
    /// </summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="placeholder">Плейсхолдер поля ввода.</param>
    /// <param name="buttonText">Текст на кнопке сохранения.</param>
    public LookupPopupViewModel(string title, string placeholder, string buttonText)
    {
        // Копируем параметры в свойства (через генератор — с уведомлением UI)
        Title = title;
        Placeholder = placeholder;
        ButtonText = buttonText;
    }

    /// <summary>
    /// Выполняет сохранение: вызывает <see cref="OnSave"/>, затем <see cref="CloseAction"/>.
    /// </summary>
    /// <returns>Задача, завершающаяся после закрытия попапа.</returns>
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Пустое имя — нечего сохранять, тихо выходим
        if (string.IsNullOrWhiteSpace(Name)) return;

        // Сохраняем (если callback задан)
        if (OnSave != null) await OnSave(Name.Trim());

        // Закрываем попап (если callback задан)
        if (CloseAction != null) await CloseAction();
    }
}
