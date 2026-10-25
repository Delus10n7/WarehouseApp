using CommunityToolkit.Maui.Views;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Maui.Views.Counterparties;

public partial class CounterpartyPopup : Popup
{
    public CounterpartyPopup(CounterpartyFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e)
    {
        await CloseAsync();
    }
}
