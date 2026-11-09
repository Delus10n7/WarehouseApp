using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel страницы списка товаров.
/// </summary>
public partial class ProductsViewModel : ObservableObject
{
    /// <summary>Значение фильтра «без фильтра по остатку».</summary>
    private const string StockFilterDefault = "По умолчанию";

    /// <summary>Значение фильтра «только товары с остатком &gt; 0».</summary>
    private const string StockFilterInStock = "В наличии";

    /// <summary>Значение фильтра «только товары с нулевым остатком».</summary>
    private const string StockFilterOutOfStock = "Нет в наличии";

    /// <summary>Сгенерированный клиент API товаров.</summary>
    private readonly IProductsClient _productsClient;

    /// <summary>Сгенерированный клиент API справочников.</summary>
    private readonly ILookupsClient _lookupsClient;

    /// <summary>Логгер для записи ошибок загрузки.</summary>
    private readonly ILogger<ProductsViewModel> _logger;

    /// <summary>Полный список товаров с сервера — «сырые» данные для клиентской фильтрации.</summary>
    private List<ProductDto> _allProducts = new();

    /// <summary>Флаг «идёт загрузка» — защита от параллельных вызовов <see cref="LoadDataAsync"/>.</summary>
    private bool _isLoading;

    /// <summary>Текст поиска по названию/артикулу.</summary>
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>Флаг обновления (для <c>RefreshView.IsRefreshing</c>).</summary>
    [ObservableProperty] private bool isRefreshing;

    /// <summary>Выбранная категория в фильтре (привязано к Picker).</summary>
    [ObservableProperty] private CategoryDto? selectedFilterCategory;

    /// <summary>Выбранный фильтр по остатку (значение из <see cref="StockOptions"/>).</summary>
    [ObservableProperty] private string? selectedStockFilter;

    /// <summary>Видимость панели фильтров (кнопка-воронка).</summary>
    [ObservableProperty] private bool isFilterVisible;

    /// <summary>Категории для выпадающего списка фильтра.</summary>
    public ObservableCollection<CategoryDto> AvailableCategories { get; } = new();

    /// <summary>Варианты фильтра по остатку — для <c>Picker.ItemsSource</c>.</summary>
    public List<string> StockOptions { get; } = new()
    {
        StockFilterDefault,
        StockFilterInStock,
        StockFilterOutOfStock
    };

    /// <summary>Отфильтрованный список товаров для <c>CollectionView</c>.</summary>
    public ObservableCollection<ProductDto> Products { get; } = new();

    /// <summary>
    /// Событие запроса открытия формы товара.
    /// Параметр — DTO для редактирования, либо <c>null</c> для создания.
    /// </summary>
    public event Func<ProductDto?, Task>? OpenProductFormRequested;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ProductsViewModel"/>.
    /// </summary>
    /// <param name="productsClient">Клиент API товаров.</param>
    /// <param name="lookupsClient">Клиент API справочников.</param>
    /// <param name="logger">Логгер для ошибок.</param>
    public ProductsViewModel(
        IProductsClient productsClient,
        ILookupsClient lookupsClient,
        ILogger<ProductsViewModel> logger)
    {
        _productsClient = productsClient;
        _lookupsClient = lookupsClient;
        _logger = logger;
    }

    /// <summary>
    /// Загружает товары и категории с сервера параллельно и применяет фильтры.
    /// </summary>
    /// <returns>Задача, завершающаяся после обновления UI.</returns>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        // Защита от параллельных запусков
        if (_isLoading) return;
        _isLoading = true;
        IsRefreshing = true;

        try
        {
            // Стартуем оба запроса одновременно
            var productsTask = _productsClient.GetAllAsync();
            var categoriesTask = _lookupsClient.GetCategoriesAsync();

            // Ждём завершения обоих
            await Task.WhenAll(productsTask, categoriesTask);

            // Материализуем товары в List (NSwag возвращает ICollection<T>)
            _allProducts = (await productsTask).ToList();

            // Заполняем категории для фильтра
            var cats = await categoriesTask;
            AvailableCategories.Clear();
            foreach (var cat in cats) AvailableCategories.Add(cat);

            // Применяем фильтры (если они уже были установлены)
            ApplyFilters();
        }
        catch (Exception ex)
        {
            // Логируем и показываем пустой список через EmptyView
            _logger.LogError(ex, "Ошибка загрузки товаров");
            _allProducts = new();
            ApplyFilters();
        }
        finally
        {
            // Гарантированно снимаем флаги
            IsRefreshing = false;
            _isLoading = false;
        }
    }

    /// <summary>При изменении текста поиска — перезапускаем фильтрацию.</summary>
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    /// <summary>При изменении выбранной категории — перезапускаем фильтрацию.</summary>
    partial void OnSelectedFilterCategoryChanged(CategoryDto? value) => ApplyFilters();

    /// <summary>При изменении фильтра по остатку — перезапускаем фильтрацию.</summary>
    partial void OnSelectedStockFilterChanged(string? value) => ApplyFilters();

    /// <summary>
    /// Переключает видимость панели фильтров.
    /// </summary>
    [RelayCommand]
    private void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    /// <summary>
    /// Применяет все активные фильтры (поиск, категория, остаток) к <see cref="_allProducts"/>.
    /// </summary>
    [RelayCommand]
    public void ApplyFilters()
    {
        // Начинаем с полного списка
        var filtered = _allProducts.AsEnumerable();

        // Фильтр 1: поиск по названию или артикулу
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Article.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // Фильтр 2: категория (по Id — надёжнее, чем сравнение объектов)
        if (SelectedFilterCategory != null)
            filtered = filtered.Where(p => p.CategoryId == SelectedFilterCategory.Id);

        // Фильтр 3: остаток
        if (SelectedStockFilter == StockFilterInStock)
            filtered = filtered.Where(p => p.Quantity > 0);
        else if (SelectedStockFilter == StockFilterOutOfStock)
            filtered = filtered.Where(p => p.Quantity == 0);

        // Переносим результат в ObservableCollection — CollectionView обновится
        Products.Clear();
        foreach (var item in filtered) Products.Add(item);
    }

    /// <summary>
    /// Сбрасывает все фильтры и применяет фильтрацию повторно.
    /// </summary>
    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedFilterCategory = null;
        SelectedStockFilter = null;

        // Явный вызов — не полагаемся только на partial-методы
        ApplyFilters();
    }

    /// <summary>
    /// Команда открытия формы товара.
    /// </summary>
    /// <param name="product"><c>null</c> — создание нового, DTO — редактирование.</param>
    /// <returns>Задача, завершающаяся после закрытия попапа.</returns>
    [RelayCommand]
    private async Task Edit(ProductDto? product)
    {
        if (OpenProductFormRequested != null)
            await OpenProductFormRequested.Invoke(product);
    }
}
