using CommunityToolkit.Maui.Views;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Maui.Views.Products;

/// <summary>
/// Всплывающее окно формы создания и редактирования товара.
/// </summary>
public partial class ProductPopup : Popup
{
    public ProductPopup(ProductFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e) => await CloseAsync();
}
