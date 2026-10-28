using CommunityToolkit.Maui.Views;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Maui.Views.Products;

/// <summary>
/// Всплывающее окно для добавления новых справочников (категорий, ед. изм.).
/// </summary>
public partial class LookupPopup : Popup
{
    public LookupPopup(LookupPopupViewModel viewModel)
    {
        InitializeComponent();
        viewModel.CloseAction = async () => await CloseAsync();
        BindingContext = viewModel;
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e) => await CloseAsync();
}
