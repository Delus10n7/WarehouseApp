using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Maui.Views.Counterparties;

public partial class CounterpartiesPage : ContentPage
{
    private readonly IPopupService _popupService;

    public CounterpartiesPage(IPopupService popupService)
    {
        InitializeComponent();
        _popupService = popupService;
    }

    private async void OnAddCounterpartyClicked(object sender, EventArgs e)
    {
        var viewModel = new CounterpartyFormViewModel(_popupService);
        var popup = new CounterpartyPopup(viewModel);

        var popupOptions = new PopupOptions
        {
            Shape = null
        };

        await this.ShowPopupAsync(popup, popupOptions);
    }

    private async void OnEditCounterpartyClicked(Counterparty counterparty)
    {
        var viewModel = new CounterpartyFormViewModel(_popupService, counterparty);
        var popup = new CounterpartyPopup(viewModel);

        var popupOptions = new PopupOptions
        {
            Shape = null
        };

        await this.ShowPopupAsync(popup, popupOptions);
    }
}