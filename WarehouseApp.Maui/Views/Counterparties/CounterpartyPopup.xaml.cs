using CommunityToolkit.Maui.Views;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Maui.Views.Counterparties;

/// <summary>
/// Всплывающее окно (Popup) с формой создания или редактирования контрагента.
/// </summary>
public partial class CounterpartyPopup : Popup
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CounterpartyPopup"/>.
    /// </summary>
    /// <param name="viewModel">ViewModel формы контрагента.</param>
    public CounterpartyPopup(CounterpartyFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Обработчик нажатия на крестик для закрытия окна.
    /// </summary>
    private async void OnCloseTapped(object? sender, TappedEventArgs e)
    {
        await CloseAsync();
    }
}
