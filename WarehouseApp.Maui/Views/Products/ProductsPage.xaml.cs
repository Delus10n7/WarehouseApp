using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Services;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Maui.Views.Products;

/// <summary>
/// Страница для управления товарами.
/// </summary>
public partial class ProductsPage : ContentPage
{
    private readonly IPopupService _popupService;
    private readonly IProductApiService _productService;
    private readonly ILookupApiService _lookupService;
    private readonly ProductsViewModel _viewModel;

    public ProductsPage(IPopupService popupService, IProductApiService productService, ILookupApiService lookupService)
    {
        InitializeComponent();
        _popupService = popupService;
        _productService = productService;
        _lookupService = lookupService;

        _viewModel = new ProductsViewModel(_productService, _lookupService);
        _viewModel.OpenProductFormRequested += OnOpenProductFormRequested;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }

    private async void OnAddProductClicked(object? sender, EventArgs e)
    {
        var formVm = new ProductFormViewModel(_popupService, _productService, _lookupService);

        formVm.ShowLookupPopupRequested = async (lookupVm) =>
        {
            var lookupPopup = new LookupPopup(lookupVm);
            await this.ShowPopupAsync(lookupPopup, new PopupOptions { Shape = null });
        };

        var popup = new ProductPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });
        await _viewModel.LoadDataAsync();
    }

    private async Task OnOpenProductFormRequested(Product? product)
    {
        ProductFormViewModel formVm;
        if (product == null)
            formVm = new ProductFormViewModel(_popupService, _productService, _lookupService);
        else
            formVm = new ProductFormViewModel(_popupService, _productService, _lookupService, product);

        formVm.ShowLookupPopupRequested = async (lookupVm) =>
        {
            var lookupPopup = new LookupPopup(lookupVm);
            await this.ShowPopupAsync(lookupPopup, new PopupOptions { Shape = null });
        };

        var popup = new ProductPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });
        await _viewModel.LoadDataAsync();
    }
}
