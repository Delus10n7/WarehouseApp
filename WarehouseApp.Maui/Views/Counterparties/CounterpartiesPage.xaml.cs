using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Services;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Maui.Views.Counterparties;

/// <summary>
/// Страница для отображения списка контрагентов и управления ими (добавление, редактирование).
/// </summary>
public partial class CounterpartiesPage : ContentPage
{
    private readonly IPopupService _popupService;
    private readonly ICounterpartyApiService _apiService;
    private readonly CounterpartiesViewModel _viewModel;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CounterpartiesPage"/>.
    /// </summary>
    /// <param name="popupService">Сервис для отображения всплывающих окон.</param>
    /// <param name="apiService">Сервис для работы с API контрагентов.</param>
    public CounterpartiesPage(IPopupService popupService, ICounterpartyApiService apiService)
    {
        InitializeComponent();
        _popupService = popupService;
        _apiService = apiService;

        _viewModel = new CounterpartiesViewModel(_apiService);
        _viewModel.EditRequested += OnEditRequested;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Вызывается при появлении страницы на экране. Загружает данные с сервера.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }

    /// <summary>
    /// Обработчик нажатия на кнопку добавления "+". Открывает модальное окно для создания контрагента.
    /// </summary>
    private async void OnAddCounterpartyClicked(object? sender, EventArgs e)
    {
        var formVm = new CounterpartyFormViewModel(_popupService, _apiService);
        var popup = new CounterpartyPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        // Обновляем список после закрытия попапа
        await _viewModel.LoadDataAsync();
    }

    /// <summary>
    /// Обработчик события запроса редактирования контрагента (нажатие на карточку).
    /// </summary>
    private async Task OnEditRequested(Counterparty counterparty)
    {
        var formVm = new CounterpartyFormViewModel(_popupService, _apiService, counterparty);
        var popup = new CounterpartyPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        // Обновляем список после закрытия попапа
        await _viewModel.LoadDataAsync();
    }
}
