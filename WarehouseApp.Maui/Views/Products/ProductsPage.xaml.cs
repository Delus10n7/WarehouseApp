using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Maui.ViewModels.Products;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.Views.Products;

/// <summary>
/// Страница для управления товарами (список, поиск, фильтры, создание/редактирование).
/// </summary>
public partial class ProductsPage : ContentPage
{
    private readonly IPopupService _popupService;
    private readonly IProductsClient _productsClient;
    private readonly ILookupsClient _lookupsClient;
    private readonly ProductsViewModel _viewModel;

    /// <summary>
    /// Инициализирует новый экземпляр страницы. Зависимости приходят через DI.
    /// </summary>
    public ProductsPage(
        IPopupService popupService,
        IProductsClient productsClient,
        ILookupsClient lookupsClient,
        ProductsViewModel viewModel)
    {
        InitializeComponent();
        _popupService = popupService;
        _productsClient = productsClient;
        _lookupsClient = lookupsClient;
        _viewModel = viewModel;

        _viewModel.OpenProductFormRequested += OnOpenProductFormRequested;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Вызывается при появлении страницы на экране. Загружает товары и категории.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnAppearing failed: {ex}");
        }
    }

    /// <summary>
    /// Обработчик кнопки «+»: открывает форму создания товара.
    /// </summary>
    private async void OnAddProductClicked(object? sender, EventArgs e)
    {
        await OpenProductFormAsync(null);
    }

    /// <summary>
    /// Обработчик события редактирования из ViewModel.
    /// </summary>
    private async Task OnOpenProductFormRequested(ProductDto? product)
    {
        await OpenProductFormAsync(product);
    }

    /// <summary>
    /// Открывает модальную форму товара (создание или редактирование).
    /// </summary>
    /// <param name="product">DTO для редактирования, либо <c>null</c> для создания.</param>
    private async Task OpenProductFormAsync(ProductDto? product)
    {
        var formVm = product is null
            ? new ProductFormViewModel(_popupService, _productsClient, _lookupsClient)
            : new ProductFormViewModel(_popupService, _productsClient, _lookupsClient, product);

        // Callback: форма запрашивает открытие вложенного попапа справочника.
        formVm.ShowLookupPopupRequested = async (lookupVm) =>
        {
            var lookupPopup = new LookupPopup(lookupVm);
            await this.ShowPopupAsync(lookupPopup, new PopupOptions { Shape = null });
        };

        // Грузим категории и единицы ДО показа попапа, чтобы Picker'ы были заполнены.
        await formVm.InitializeAsync();

        var popup = new ProductPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        // Перезагружаем список после закрытия.
        await _viewModel.LoadDataAsync();
    }
}
