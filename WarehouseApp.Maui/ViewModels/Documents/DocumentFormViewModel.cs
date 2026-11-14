using System.Collections.ObjectModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarehouseApp.Application.Dtos.CommonDtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities.Enums;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Documents;

/// <summary>
/// ViewModel модальной формы создания и редактирования документа.
/// </summary>
public partial class DocumentFormViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly IDocumentsClient _documentsClient;
    private readonly IProductsClient _productsClient;
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly WareDocumentDto? _entityToUpdate;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = string.Empty;
    [ObservableProperty] private Color buttonColor = Colors.Transparent;

    // Тип документа (Поступление / Продажа)
    [ObservableProperty] private bool isReceipt = true;

    [ObservableProperty] private ProductDto? selectedProduct;
    [ObservableProperty] private CounterpartyDto? selectedCounterparty;
    [ObservableProperty] private string quantityText = string.Empty;
    [ObservableProperty] private string amountText = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<CounterpartyDto> Counterparties { get; } = new();

    public bool IsEditMode => _entityToUpdate != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Заголовок для поля выбора контрагента. Возвращает "Поставщик" или "Покупатель" без звездочки.
    /// </summary>
    public string CounterpartyLabel => IsReceipt ? "Поставщик" : "Покупатель";

    /// <summary>
    /// Динамический заголовок для поля количества. Добавляет единицу измерения, если выбран товар.
    /// </summary>
    public string QuantityLabel => SelectedProduct != null && !string.IsNullOrWhiteSpace(SelectedProduct.UnitOfMeasureName)
        ? $"Количество, {SelectedProduct.UnitOfMeasureName}"
        : "Количество";

    public DocumentFormViewModel(
        IPopupService popupService,
        IDocumentsClient documentsClient,
        IProductsClient productsClient,
        ICounterpartiesClient counterpartiesClient)
    {
        _popupService = popupService;
        _documentsClient = documentsClient;
        _productsClient = productsClient;
        _counterpartiesClient = counterpartiesClient;

        Title = "Создание документа";
        ButtonText = "Добавить документ";
        ButtonColor = Color.FromArgb("#4CAF50");
    }

    public DocumentFormViewModel(
        IPopupService popupService,
        IDocumentsClient documentsClient,
        IProductsClient productsClient,
        ICounterpartiesClient counterpartiesClient,
        WareDocumentDto existing) : this(popupService, documentsClient, productsClient, counterpartiesClient)
    {
        _entityToUpdate = existing;

        Title = existing.Type == WareDocumentType.Receipt ? "Документ Поступление" : "Документ Продажа";
        ButtonText = "Изменить документ";
        ButtonColor = Color.FromArgb("#1976D2");

        IsReceipt = existing.Type == WareDocumentType.Receipt;

        // Для упрощения берем первый товар (так как в UI поддерживаем 1 строку)
        var item = existing.Items.FirstOrDefault();
        if (item != null)
        {
            QuantityText = item.Quantity.ToString("G29");
            AmountText = (item.Quantity * item.Price).ToString("G29");
        }
    }

    /// <summary>
    /// Загружает справочники товаров и контрагентов.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var productsTask = _productsClient.GetAllAsync();
            var counterpartiesTask = _counterpartiesClient.GetAllAsync();
            await Task.WhenAll(productsTask, counterpartiesTask);

            foreach (var p in await productsTask) Products.Add(p);
            foreach (var c in await counterpartiesTask) Counterparties.Add(c);

            if (IsEditMode && _entityToUpdate != null)
            {
                var item = _entityToUpdate.Items.FirstOrDefault();
                if (item != null)
                    SelectedProduct = Products.FirstOrDefault(p => p.Id == item.ProductId);

                SelectedCounterparty = Counterparties.FirstOrDefault(c => c.Id == _entityToUpdate.CounterpartyId);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка загрузки справочников: " + ex.Message;
        }
    }

    partial void OnIsReceiptChanged(bool value)
    {
        OnPropertyChanged(nameof(CounterpartyLabel));
        if (!IsEditMode)
        {
            Title = value ? "Создание документа Поступление" : "Создание документа Продажа";
        }
    }

    /// <summary>
    /// Обновляет заголовок количества при смене выбранного товара.
    /// </summary>
    partial void OnSelectedProductChanged(ProductDto? value)
    {
        OnPropertyChanged(nameof(QuantityLabel));
    }

    /// <summary>
    /// Переключает тип документа (Поступление/Продажа).
    /// </summary>
    [RelayCommand]
    private void ToggleType(string type)
    {
        IsReceipt = type == "Receipt";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProduct == null || SelectedCounterparty == null ||
            !decimal.TryParse(QuantityText, out var qty) || qty <= 0 ||
            !decimal.TryParse(AmountText, out var totalAmount) || totalAmount < 0)
        {
            ErrorMessage = "Заполните все поля корректными значениями.";
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            // Вычисляем цену за единицу на основе общей суммы и количества
            decimal price = totalAmount / qty;
            var type = IsReceipt ? WareDocumentType.Receipt : WareDocumentType.Sale;

            var dto = new WareDocumentCreateDto
            {
                Number = IsEditMode ? _entityToUpdate!.Number : $"DOC-{DateTime.Now:yyyyMMddHHmmss}",
                Date = IsEditMode ? _entityToUpdate!.Date : DateTime.UtcNow,
                Type = type,
                CounterpartyId = SelectedCounterparty.Id,
                Items = new List<WareDocumentItemCreateDto>
                {
                    new() { ProductId = SelectedProduct.Id, Quantity = qty, Price = price }
                }
            };

            if (IsEditMode && _entityToUpdate != null)
                await _documentsClient.UpdateAsync(_entityToUpdate.Id, dto);
            else
                await _documentsClient.CreateAsync(dto);

            if (Shell.Current != null)
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
        }
        catch (ApiException<ProblemDetails> ex)
        {
            // Извлекаем красивое сообщение об ошибке (например, "Недостаточно товара...")
            ErrorMessage = ex.Result?.Detail ?? ex.Result?.Title ?? "Произошла ошибка при сохранении документа.";
        }
        catch (ApiException ex)
        {
            ErrorMessage = "Ошибка " + ex.StatusCode.ToString() + ". Проверьте введенные данные.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
