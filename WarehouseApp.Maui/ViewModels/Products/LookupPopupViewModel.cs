using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel для универсального всплывающего окна добавления справочника (категории или ед. изм.).
/// </summary>
public partial class LookupPopupViewModel : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string placeholder = string.Empty;
    [ObservableProperty] private string buttonText = string.Empty;
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Коллбэк для сохранения созданной сущности.</summary>
    public Func<string, Task>? OnSave { get; set; }

    /// <summary>Коллбэк для закрытия окна.</summary>
    public Func<Task>? CloseAction { get; set; }

    public LookupPopupViewModel(string title, string placeholder, string buttonText)
    {
        Title = title;
        Placeholder = placeholder;
        ButtonText = buttonText;
        Name = string.Empty;
    }

    /// <summary>
    /// Выполняет сохранение и закрывает окно.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        if (OnSave != null) await OnSave(Name.Trim());
        if (CloseAction != null) await CloseAction();
    }
}
