using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Interfaces.Abstractions;
using WarehouseApp.Maui.Services;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Maui.Views.Counterparties;

public partial class CounterpartiesPage : ContentPage
{
    private readonly IPopupService _popupService;
    private readonly ICounterpartyApiService _apiService;
    private readonly CounterpartiesViewModel _viewModel;

    public CounterpartiesPage(IPopupService popupService, ICounterpartyApiService apiService)
    {
        InitializeComponent();
        _popupService = popupService;
        _apiService = apiService;

        _viewModel = new CounterpartiesViewModel(_apiService);
        _viewModel.EditRequested += OnEditRequested;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }

    private async void OnAddCounterpartyClicked(object sender, EventArgs e)
    {
        var formVm = new CounterpartyFormViewModel(_popupService, _apiService);
        var popup = new CounterpartyPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        await _viewModel.LoadDataAsync();
    }

    private async Task OnEditRequested(Counterparty counterparty)
    {
        var formVm = new CounterpartyFormViewModel(_popupService, _apiService, counterparty);
        var popup = new CounterpartyPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        await _viewModel.LoadDataAsync();
    }
}