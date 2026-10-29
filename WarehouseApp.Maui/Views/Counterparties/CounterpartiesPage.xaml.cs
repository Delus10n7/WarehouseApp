using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
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
    /// Инициализирует новый экземпляр страницы. Все зависимости приходят через DI.
    /// </summary>
    /// <param name="popupService">Сервис показа модальных окон.</param>
    /// <param name="apiService">Клиент API контрагентов (нужен для открытия формы).</param>
    /// <param name="viewModel">ViewModel страницы (регистрируется в DI).</param>
    public CounterpartiesPage(
        IPopupService popupService,
        ICounterpartyApiService apiService,
        CounterpartiesViewModel viewModel)
    {
        InitializeComponent();
        _popupService = popupService;
        _apiService = apiService;
        _viewModel = viewModel;

        _viewModel.EditRequested += OnEditRequested;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Вызывается при появлении страницы на экране. Загружает контрагентов с сервера.
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
    /// Обработчик нажатия на кнопку «+»: открывает форму создания контрагента.
    /// </summary>
    private async void OnAddCounterpartyClicked(object? sender, EventArgs e)
    {
        await OpenFormAsync(null);
    }

    /// <summary>
    /// Обработчик события редактирования: открывает форму с существующим контрагентом.
    /// </summary>
    private async Task OnEditRequested(CounterpartyDto counterparty)
    {
        await OpenFormAsync(counterparty);
    }

    /// <summary>
    /// Открывает модальную форму контрагента (создание или редактирование)
    /// и после закрытия перезагружает список.
    /// </summary>
    /// <param name="existing">Существующий DTO для редактирования, либо <c>null</c> для создания.</param>
    private async Task OpenFormAsync(CounterpartyDto? existing)
    {
        var formVm = existing is null
            ? new CounterpartyFormViewModel(_popupService, _apiService)
            : new CounterpartyFormViewModel(_popupService, _apiService, existing);

        var popup = new CounterpartyPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        // Перезагружаем список после закрытия — простая, но рабочая стратегия.
        await _viewModel.LoadDataAsync();
    }
}
