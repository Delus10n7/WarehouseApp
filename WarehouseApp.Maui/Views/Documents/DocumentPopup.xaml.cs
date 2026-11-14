using CommunityToolkit.Maui.Views;
using WarehouseApp.Maui.ViewModels.Documents;

namespace WarehouseApp.Maui.Views.Documents;

/// <summary>
/// Всплывающее окно формы создания и редактирования документа.
/// </summary>
public partial class DocumentPopup : Popup
{
    public DocumentPopup(DocumentFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e) => await CloseAsync();
}
